using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinanceManagement.Binance;
using FinanceManagement.Common;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Controllers
{
    [Route("api/v1")]
    public class ChartingController: ControllerBase
    {
        private readonly BinanceApi _api;

        public ChartingController(BinanceApi api)
        {
            _api = api;
        }
        
        [HttpGet, Route("candles")]
        public async Task<IEnumerable<Candle>> GetCandles([FromQuery] string instrument, [FromQuery] Interval interval)
        {
            var candles = await _api.GetCandlesFromCsvAsync("ETHBUSD-1m-2021-05.csv");
            var store = new CandlesStore();
            foreach (var candle in candles)
            {
                store.AddCandle(candle);
            }
            return store.Candles[interval];
        }
    }
}