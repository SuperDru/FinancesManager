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
        
        [HttpGet, Route("{instrument:required}/prices")]
        public async Task<IEnumerable<ChartingPrice>> GetLinePrices([FromRoute] string instrument)
        {
            // var candles = await _api.GetCandlesAsync(instrument.ToUpper());
            // return candles.Select(_ => new ChartingPrice
            // {
            //     Price = _.Close,
            //     Time = _.Time
            // });
            var candles = await _api.GetCandlesFromCsvAsync("ETHBUSD-1m-2021-05.csv");
            return candles.Select(_ => new ChartingPrice
            {
                Price = _.Close,
                Time = _.Time
            });
        }
    }
}