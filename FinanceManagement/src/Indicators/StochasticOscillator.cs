using System.Collections.Generic;
using System.Linq;
using FinanceManagement.Common;

namespace FinanceManagement.Indicators
{
    public class StochasticOscillator: ICandleIndicator
    {
        public List<object> Series { get; } = new();

        private LinkedList<Candle> _values;

        private int _fastLength;

        private MovingAverage _movingAverage;

        public decimal KValue;
        public decimal DValue;

        public StochasticOscillator(int fastLength, int slowLength)
        {
            _fastLength = fastLength;
            _values = new LinkedList<Candle>();
            _movingAverage = new MovingAverage(slowLength);
        }

        public void Push(Candle candle, bool changed = false)
        {
            if (_fastLength == 0) return;

            if (changed && _values.Count >= 1)
            {
                _values.RemoveLast();
            }
            else if (_values.Count == _fastLength)
            {
                _values.RemoveFirst();
            }
            _values.AddLast(candle);
            
            var highest = _values.Max(x => x.High);
            var lowest = _values.Min(x => x.Low);

            var kValue = highest == lowest ? 0 : (candle.Close - lowest) / (highest - lowest) * 100;
            
            _movingAverage.Push(kValue, changed);
            var dValue = _movingAverage.Value;

            KValue = kValue;
            DValue = dValue;

            Series.Add((KValue = kValue, DValue = dValue));
        }
    }
}
