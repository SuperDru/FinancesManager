using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinanceManagement.Binance;
using FinanceManagement.Bot.Backtesting;
using FinanceManagement.Bot.Strategies;
using FinanceManagement.Common;
using Microsoft.Extensions.Hosting;

namespace FinanceManagement.Bot
{
    public class BotHostedService: IHostedService
    {
        private readonly BinanceApi _api;

        public BotHostedService(BinanceApi api) => _api = api;
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var candles1 = await _api.GetCandlesFromCsvAsync("ETHBUSD-1m-2021-01.csv");
            var candles2 = await _api.GetCandlesFromCsvAsync("ETHBUSD-1m-2021-02.csv");
            var candles3 = await _api.GetCandlesFromCsvAsync("ETHBUSD-1m-2021-03.csv");
            var candles4 = await _api.GetCandlesFromCsvAsync("ETHBUSD-1m-2021-04.csv");
            var candles5 = await _api.GetCandlesFromCsvAsync("ETHBUSD-1m-2021-05.csv");
            var candles6 = await _api.GetCandlesFromCsvAsync("ETHBUSD-1m-2021-06.csv");
            var strategy = new StochLessMoreStrategy("ETHBUSD", 80m, 20m, 5m, 10m, 5m);
            var candlesLoadingTime = stopwatch.ElapsedMilliseconds.ToString();

            stopwatch.Restart();

            var candles = new List<Candle>()
                // .Concat(candles1)
                // .Concat(candles2)
                // .Concat(candles3)
                // .Concat(candles4)
                .Concat(candles5)
                // .Concat(candles6)
                .ToList();
            var context = new Backtest("ETHBUSD", strategy);
            var result = await context.Process(candles);

            var backtestTime = stopwatch.ElapsedMilliseconds.ToString();
            stopwatch.Stop();

            Logger.Warning(result.ToString());
            Logger.Warning($"loading time: {candlesLoadingTime}");
            Logger.Warning($"backtest time: {backtestTime}");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}