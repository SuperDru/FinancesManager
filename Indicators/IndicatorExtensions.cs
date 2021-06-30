﻿using System.Collections.Generic;
using System.Linq;
using Tinkoff.Trading.OpenApi.Models;

namespace FinancesManager.Indicators
{
    public static class IndicatorExtensions
    {
        public static decimal MA(this IEnumerable<CandlePayload> candles, int length) =>
            candles.Select(x => x.Close).MA(length);

        public static decimal MA(this IEnumerable<decimal> candles, int length)
        {
            var ma = new MovingAverage(length);

            foreach (var candle in candles.TakeLast(length))
            {
                ma.Push(candle);
            }

            return ma.Value ?? 0;
        }

        public static decimal EMA(this IEnumerable<CandlePayload> candles, int length)
        {
            var ema = new ExponentialMovingAverage(length);

            foreach (var candle in candles.TakeLast(length))
            {
                ema.Push(candle.Close);
            }

            return ema.Value ?? 0;
        }

        public static decimal STOCH(this IEnumerable<CandlePayload> candles, int kLength = 14, int dLength = 3)
        {
            var stoch = new StochasticOscillator(kLength, dLength);

            foreach (var candle in candles.TakeLast(kLength + dLength))
            {
                stoch.Push(candle);
            }

            return stoch.DValue ?? 0;
        }
    }
}
