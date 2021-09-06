using System;
using System.Collections.Generic;
using FinanceManagement.Common;
using Force.DeepCloner;

namespace FinanceManagement.Indicators
{
    public enum CandleSource
    {
        Ohlcv = 1,
        Ohlc = 2,
        Hlc = 3,
        Hl = 4,
        Close = 5,
        Open = 6,
        Low = 7,
        High = 8,
        Volume = 9
    }
    
    public class IndicatorsCalculator: IObserver<Candle>
    {
        private Dictionary<Interval, Candle> LastCandles { get; } = new();
        
        private Dictionary<Guid, CandleSource> SourcesByGuid { get; } = new();
        private Dictionary<Guid, IIndicator> IndicatorsByGuid { get; } = new();
        private Dictionary<Guid, IIndicator> IndicatorsSaveByGuid { get; } = new();
        private Dictionary<Interval, List<Guid>> IndicatorGuidsByInterval { get; } = new();

        public IndicatorsCalculator(IObservable<Candle> store)
        {
            store.Subscribe(this);
        }

        public T RequireIndicator<T>(T indicator, Interval interval = Interval.Minute, CandleSource source = CandleSource.Ohlc)
            where T: IIndicator
        {
            IndicatorGuidsByInterval.TryAdd(interval, new List<Guid>());

            var guid = Guid.NewGuid();
            
            IndicatorGuidsByInterval[interval].Add(guid);
            IndicatorsByGuid[guid] = indicator;
            SourcesByGuid[guid] = source;

            return indicator;
        }

        public void OnNext(Candle candle)
        {
            var interval = candle.Interval;
            foreach (var guid in IndicatorGuidsByInterval.GetValueOrDefault(interval) ?? new List<Guid>())
            {
                var indicator = IndicatorsByGuid[guid];
                var source = SourcesByGuid[guid];

                var changed = LastCandles.GetValueOrDefault(interval)?.Time == candle.Time;
                Push(indicator, changed, candle, source);
            }
            LastCandles[interval] = candle;
        }

        private static void Push(IIndicator indicator, bool changed, Candle candle, CandleSource source)
        {
            switch (indicator)
            {
                case IDecimalIndicator t: t.Push((decimal) DeriveBySource(candle, source), changed); break;
                case ICandleIndicator t: t.Push((Candle) DeriveBySource(candle, source), changed); break;
            }
        }

        private static object DeriveBySource(Candle candle, CandleSource source) => source switch
        {
            CandleSource.Ohlcv => candle,
            CandleSource.Ohlc => candle,
            CandleSource.Hlc => candle,
            CandleSource.Hl => candle,
            CandleSource.Volume => candle.Volume,
            CandleSource.Open => candle.Open,
            CandleSource.High => candle.High,
            CandleSource.Low => candle.Low,
            CandleSource.Close => candle.Close,
            _ => throw new ArgumentOutOfRangeException(nameof(source))
        };
        
        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }
    }
}