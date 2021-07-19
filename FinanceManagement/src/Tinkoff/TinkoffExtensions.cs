using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinanceManagement.Income;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;

namespace FinanceManagement.Tinkoff
{
    public static class TinkoffExtensions
    {
        public static async Task<decimal> GetTotalClosedPositionsIncome(this Context context, params IncomeContext[] marketContexts)
        {
            var currencies = await context.MarketCurrenciesAsync();
            var currency = currencies.Instruments.First(x => x.Ticker == "USD000UTSTOM");
            var candles = await context.MarketCandlesAsync(currency.Figi, DateTime.Now.AddDays(-7), DateTime.Now, CandleInterval.Day);
            var usdPrice = candles.Candles.Last().Close;

            return marketContexts.Sum(marketsContext => marketsContext.Currency == Currency.Rub
                ? marketsContext.ClosedPositionsTotalIncome
                : marketsContext.ClosedPositionsTotalIncome * usdPrice);
        }

        public static async Task<CandleList> GetCandlesByFigi(this Context context, string figi, CandleInterval interval = CandleInterval.Minute, DateTime? from = default, DateTime? to = default) =>
            await context.MarketCandlesAsync(figi, from ?? DateTime.Today, to ?? DateTime.Now, interval);

        public static async Task<CandleList> GetCandles(this Context context, string ticker,
            CandleInterval interval = CandleInterval.Minute, DateTime? from = default, DateTime? to = default)
        {
            var market = await context.MarketSearchByTickerAsync(ticker);

            if (market == null || !market.Instruments.Any())
            {
                throw new KeyNotFoundException("Ticker isn't supported.");
            }

            return await context.GetCandlesByFigi(market.Instruments.First().Figi, interval, from, to);
        }

        public static async Task<CandlePayload> GetLastCandleByFigi(this Context context, string figi, CandleInterval interval = CandleInterval.Minute)
        {
            var now = DateTime.Now;
            DateTime from, to;
            switch (interval)
            {
                case CandleInterval.Minute: from = now.AddMinutes(-5); to = now.AddMinutes(5); break;
                case CandleInterval.Day: from = now.AddDays(-5); to = now.AddDays(5); break;
                default: throw new NotImplementedException();
            }
            var candles = await context.MarketCandlesAsync(figi, from, to, interval);
            return candles.Candles.Count == 0 ? null : candles.Candles.Last();
        }

        public static async Task<CandlePayload> GetLastCandle(this Context context, string ticker, CandleInterval interval = CandleInterval.Minute)
        {
            var market = await context.MarketSearchByTickerAsync(ticker);

            if (market == null || !market.Instruments.Any())
            {
                throw new KeyNotFoundException("Ticker isn't supported.");
            }

            return await context.GetLastCandleByFigi(market.Instruments.First().Figi, interval);
        }
    }
}
