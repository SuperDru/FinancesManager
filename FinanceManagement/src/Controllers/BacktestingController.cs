using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinanceManagement.Binance;
using FinanceManagement.Bot;
using FinanceManagement.Bot.Backtesting;
using FinanceManagement.Common;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Controllers
{
    [Route("api/v1")]
    public class BacktestingController: ControllerBase
    {
        [HttpGet("backtest")]
        public async Task Backtest(
            [FromQuery] string instrument,
            [FromQuery] string strategyName,
            [FromServices] BinanceApi api)
        {
            var strategy = new StochLessMoreStrategy(instrument, 80m, 20m, 5m);

            var candles = new List<Candle>();
            var from = DateTime.UtcNow.AddDays(-3);
            while (from <= DateTime.UtcNow.AddHours(-1))
            {
                candles.AddRange(await api.GetCandlesAsync("ETHBUSD", Interval.Minute, from));
                from = candles.Last().Time.AddMinutes(1);
            }

            var context = new Backtest(instrument, strategy);
            context.Process(candles);
        }
    }
}