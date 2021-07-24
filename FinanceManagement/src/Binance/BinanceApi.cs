using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Binance.Net;
using Binance.Net.Enums;
using Binance.Net.Objects.Spot;
using CryptoExchange.Net.Authentication;
using CsvHelper;
using CsvHelper.Configuration;
using FinanceManagement.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace FinanceManagement.Binance
{
    public class BinanceApi: IFinanceDataApi
    {
        private readonly BinanceClient _client;
        private readonly string Directory;

        public BinanceApi(IOptions<BinanceOptions> binanceOptions, IWebHostEnvironment hostingEnvironment)
        {
            Directory = hostingEnvironment.ContentRootPath;
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
                Interval = interval,
                Time = _.OpenTime,
                Open = _.Open,
                Close = _.Close,
                Low = _.Low,
                High = _.High,
                Volume = _.BaseVolume
            }).ToList();
        }

        public async Task<List<Candle>> GetCandlesFromCsvAsync(string fileName)
        {
            using var reader = new StreamReader(Path.Combine(Directory, "HistoricalData", fileName));
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            });
            
            var candles = new List<Candle>();
            await foreach (var candle in csv.GetRecordsAsync<BinanceHistoricalData>())
            {
                candles.Add(new Candle
                {
                    Interval = Interval.Minute,
                    Time = DateTimeOffset.FromUnixTimeMilliseconds(candle.OpenTime).UtcDateTime,
                    Open = candle.Open,
                    Close = candle.Close,
                    High = candle.High,
                    Low = candle.Low,
                    Volume = candle.Volume
                });
            }
            return candles;
        }
    }
}