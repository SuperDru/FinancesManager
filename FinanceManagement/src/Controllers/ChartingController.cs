using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinanceManagement.Binance;
using FinanceManagement.Common;
using FinanceManagement.Indicators;
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
            var candles = await _api.GetCandlesFromCsvAsync("ETHBUSD-1m-2021-06.csv");
            var store = new CandlesStore();
            foreach (var candle in candles)
            {
                store.AddCandle(candle);
            }
            return store.Candles[interval];
        }
        
        [HttpGet, Route("indicator")]
        public async Task<IIndicator> GetIndicator([FromQuery] string instrument, [FromQuery] Interval interval)
        {
            var candles = await _api.GetCandlesFromCsvAsync("ETHBUSD-1m-2021-06.csv");
            var store = new CandlesStore();
            foreach (var candle in candles)
            {
                store.AddCandle(candle);
            }

            var hour = store.Candles[interval];
            var ma = new MovingAverage(50);
            foreach (var candle in hour)
            {
                ma.Push(candle);
            }
            return ma;
        }
    }
}