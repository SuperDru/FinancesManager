using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinancesManager.Income;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;

namespace FinancesManager.Common
{
    public static class ContextExtensions
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
            var candles = await context.MarketCandlesAsync(figi, now.AddMinutes(-5), DateTime.Now.AddMinutes(5), interval);
            return candles.Candles.Last();
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
