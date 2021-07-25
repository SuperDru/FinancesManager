using System;
using System.Collections.Generic;
using System.Linq;

namespace FinanceManagement.Common
{
    public class CandlesStore
    {
        private Dictionary<Interval, List<Candle>> Candles { get; } = Enum
            .GetValues(typeof(Interval))
            .Cast<Interval>()
            .ToDictionary(_ => _, _ => new List<Candle>());

        public List<Candle> Minute => Candles[Interval.Minute];
        public List<Candle> Minute3 => Candles[Interval.Minute3];
        public List<Candle> Minute5 => Candles[Interval.Minute5];
        public List<Candle> Minute15 => Candles[Interval.Minute15];
        public List<Candle> Minute30 => Candles[Interval.Minute30];
        public List<Candle> Hour => Candles[Interval.Hour];
        public List<Candle> Hour4 => Candles[Interval.Hour4];
        public List<Candle> Day => Candles[Interval.Day];
        public List<Candle> Week => Candles[Interval.Week];
        public List<Candle> Month => Candles[Interval.Month];

        /// <summary>
        /// Add minute candle to the store
        /// </summary>
        /// <param name="candle"></param>
        /// <exception cref="ArgumentException"></exception>
        public void AddCandle(Candle candle)
        {
            if (candle.Interval != Interval.Minute)
                throw new ArgumentException("Only minute candles can be added to the store");
            
            foreach (var interval in Candles.Keys)
            {
                UpdateCandles(interval, candle, interval.StartTime(candle.Time));
            }
        }

        private void UpdateCandles(Interval interval, Candle candle, DateTime startTime)
        {
            var candles = Candles[interval];
            var last = candles.LastOrDefault();
            
            if (last?.Time > startTime) return;
            
            if (last == null || last.Time < startTime)
            {
                candles.Add(new Candle
                {
                    Interval = interval,
                    Open = candle.Open,
                    High = candle.High,
                    Low = candle.Low,
                    Close = candle.Close,
                    Volume = candle.Volume,
                    Time = startTime
                });
                return;
            }

            last.High = Math.Max(last.High, candle.High);
            last.Low = Math.Min(last.Low, candle.Low);
            last.Volume += candle.Volume;
            last.Close = candle.Close;
        }
    }
}