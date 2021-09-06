using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinanceManagement.Binance;
using FinanceManagement.Bot;
using FinanceManagement.Bot.Backtesting;
using FinanceManagement.Bot.Strategies;
using FinanceManagement.Common;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace FinanceManagement.Controllers
{
    [Route("api/v1")]
    public class BacktestingController: ControllerBase
    {
        [HttpGet("backtest")]
        public async Task<BacktestResult> Backtest(
            [FromQuery] string instrument,
            [FromQuery] string strategyName,
            [FromServices] BinanceApi api)
        {
            instrument = instrument.ToUpper();
            
            var candles = await api.GetCandlesFromCsvAsync("ETHBUSD-1m-2021-06.csv");
            var options = new StochLessMoreStrategyOptions
            {
                Instrument = instrument,
                Overbought = 80m,
                Oversold = 20m,
                Threshold = 5m,
                TakeProfit = 10m,
                StopLoss = 5m,
            };
            var strategy = new StochLessMoreStrategy(options);

            var context = new Backtest(instrument, strategy);
            return context.Process(candles);
        }
    }
}