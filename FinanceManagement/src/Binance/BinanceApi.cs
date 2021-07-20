using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Binance.Net;
using Binance.Net.Enums;
using Binance.Net.Objects.Spot;
using CryptoExchange.Net.Authentication;
using FinanceManagement.Common;
using Microsoft.Extensions.Options;

namespace FinanceManagement.Binance
{
    public class BinanceApi: IFinanceDataApi
    {
        private readonly BinanceClient _client;

        public BinanceApi(IOptions<BinanceOptions> binanceOptions)
        {
            var options = binanceOptions?.Value ?? throw new ArgumentException("Options cannot be null", nameof(binanceOptions));

            _client = new BinanceClient(new BinanceClientOptions
            {
                ApiCredentials = new ApiCredentials(options.ApiKey, options.SecretKey)
            });
        }

        public async Task<List<Candle>> GetCandlesAsync(string instrument, Interval interval = Interval.Minute, DateTime? from = default, DateTime? to = default)
        {
            var res = await _client.Spot.Market.GetKlinesAsync(instrument, KlineInterval.OneMinute, from, to, 1000);
            return res.Data.Select(_ => new Candle
            {
                Time = _.OpenTime,
                Open = _.Open,
                Close = _.Close,
                Low = _.Low,
                High = _.High,
                Volume = _.BaseVolume
            }).ToList();
        }
    }
}