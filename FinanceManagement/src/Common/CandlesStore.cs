using System;
using System.Collections.Generic;
using System.Linq;

namespace FinanceManagement.Common
{
    public class CandlesStore: IObservable<Candle>
    {
        public DateTime Time => Minute.LastOrDefault()?.Time ?? default;
        
        public Dictionary<Interval, List<Candle>> Candles { get; } = Enum
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

        private List<IObserver<Candle>> _observers;

        public IDisposable Subscribe(IObserver<Candle> observer)
        {
            _observers ??= new List<IObserver<Candle>>();
            _observers.Add(observer);

            return new UnsubscribeContext<Candle>(_observers, observer);
        }

        /// <summary>
        /// Add minute candle to the store
        /// </summary>
        /// <param name="candle"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>True if new candle is added</returns>
        public bool AddCandle(Candle candle)
        {
            if (candle.Interval != Interval.Minute)
                throw new ArgumentException("Only minute candles can be added to the store");

            var previousTime = Time;
            
            foreach (var interval in Candles.Keys)
            {
                if (!UpdateCandles(interval, candle, interval.StartTime(candle.Time)))
                    continue;
                
                if (_observers == null) continue;

                foreach (var observer in _observers)
                {
                    var intervalCandle = Candles[interval].Last();
                    observer.OnNext(intervalCandle);
                }
            }

            return Time > previousTime;
        }

        private bool UpdateCandles(Interval interval, Candle candle, DateTime startTime)
        {
            var candles = Candles[interval];
            var last = candles.LastOrDefault();
            
            if (last?.Time > startTime) return false;

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
                return true;
            }

            last.High = Math.Max(last.High, candle.High);
            last.Low = Math.Min(last.Low, candle.Low);
            last.Volume += candle.Volume;
            last.Close = candle.Close;

            return true;
        }
    }
}