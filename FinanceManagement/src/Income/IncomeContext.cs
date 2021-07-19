using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using ArgumentException = System.ArgumentException;

namespace FinanceManagement.Income
{
    public class IncomeContext
    {
        public Currency Currency { get; }

        public List<Income> ClosedPositionsIncome { get; set; }
        public List<Income> OpenedPositionsIncome { get; set; }
        public decimal ClosedPositionsTotalIncome { get; set; }
        public decimal OpenedPositionsTotalIncome { get; set; }
        public decimal Fee { get; set; }

        private Context Context { get; }

        public IncomeContext(Context tradingContext, Currency currency)
        {
            Context = tradingContext;
            Currency = currency;
        }

        public async Task Calculate(DateTime fromDate, DateTime? toDate = null)
        {
            var account = await Context.PortfolioAsync();
            var currentStock = account.Positions.ToDictionary(x => x.Figi, x => x);

            var allOperations = await Context.OperationsAsync(fromDate, toDate ?? DateTime.Now, null);
            var stockOperations = allOperations
                .Where(x => x.Currency == Currency
                            && x.InstrumentType != InstrumentType.Currency
                            && x.OperationType != ExtendedOperationType.PayIn
                            && x.OperationType != ExtendedOperationType.PayOut
                            && x.OperationType != ExtendedOperationType.ServiceCommission
                            && x.Figi != null
                            && x.Status == OperationStatus.Done)
                .OrderBy(x => x.Date)
                .ToList();

            Fee = stockOperations
                .Where(x => x.OperationType == ExtendedOperationType.BrokerCommission)
                .Sum(x => -x.Payment);

            var stockIncome = new Dictionary<string, (Income Income, Dictionary<decimal, int> Current)>();
            foreach (var operation in stockOperations)
            {
                var figi = operation.Figi;
                var price = operation.Price;
                var quantity = operation.Quantity;
                var payment = operation.Payment;

                if (stockIncome.TryGetValue(figi, out var value))
                {
                    var income = value.Income;
                    var current = value.Current;

                    if (operation.OperationType != ExtendedOperationType.Buy &&
                        operation.OperationType != ExtendedOperationType.Sell)
                    {
                        income.Amount += payment;
                        continue;
                    }

                    if (operation.OperationType == ExtendedOperationType.Buy)
                    {
                        current[price] = current.ContainsKey(price) ? current[price] + quantity : quantity;
                        continue;
                    }

                    while (quantity != 0)
                    {
                        var bestPrice = price - current.Keys.Max(x => price - x);
                        if (current[bestPrice] <= quantity)
                        {
                            income.Amount += (price - bestPrice) * current[bestPrice];
                            quantity -= current[bestPrice];
                            current.Remove(bestPrice);
                        }
                        else
                        {
                            income.Amount += (price - bestPrice) * quantity;
                            current[bestPrice] -= quantity;
                            quantity = 0;
                        }
                    }
                }
                else
                {
                    var instrument = await Context.MarketSearchByFigiAsync(figi);
                    var current = new Dictionary<decimal, int>();

                    if (operation.OperationType == ExtendedOperationType.Buy)
                    {
                        current[price] = quantity;
                    }

                    if (operation.OperationType == ExtendedOperationType.Sell)
                    {
                        throw new ArgumentException("Sell order can't be now");
                    }


                    stockIncome[operation.Figi] = (new Income
                    {
                        Figi = figi,
                        Currency = operation.Currency,
                        Ticker = instrument.Ticker,
                        Name = instrument.Name,
                        Amount = operation.OperationType != ExtendedOperationType.Buy ? operation.Payment : 0
                    }, current);
                }
            }

            ClosedPositionsIncome = stockIncome.Values
                // .Where(x => !currentStock.ContainsKey(x.Income.Ticker))
                .Select(x => x.Income)
                .ToList();
            ClosedPositionsTotalIncome = ClosedPositionsIncome.Sum(x => x.Amount);

            OpenedPositionsIncome = stockIncome.Values
                .Where(x => currentStock.ContainsKey(x.Income.Figi))
                .Select(x => x.Income)
                .ToList();
            //
            // foreach (var income in OpenedPositionsIncome)
            // {
            //     var candles = await Context.MarketCandlesAsync(income.Ticker, DateTime.Today.AddDays(-7), DateTime.Now, CandleInterval.Day);
            //     var price = candles.Candles.Last().Close;
            // }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine($"---- Income, {Currency} currency ----");
            builder.AppendLine($"\tFee: {Fee} {Currency};");
            builder.AppendLine("\t-- Income for closed positions --");
            foreach (var income in ClosedPositionsIncome)
            {
                builder.AppendLine($"\t\t - {income.Ticker}: {income.Amount} {income.Currency};");
            }
            builder.AppendLine($"\tTotal income for closed positions: {ClosedPositionsTotalIncome} {Currency};");
            builder.AppendLine();

            return builder.ToString();
        }
    }
}
