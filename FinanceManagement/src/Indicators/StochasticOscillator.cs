using System.Collections.Generic;
using System.Linq;
using FinanceManagement.Common;

namespace FinanceManagement.Indicators
{
    public class StochasticOscillator
    {
        private readonly Queue<Candle> _values;
        private readonly Queue<decimal> _kValues;

        private readonly int _fastLength;
        private readonly int _slowLength;

        private readonly MovingAverage _movingAverage;

        public decimal KValue;
        public decimal DValue;
        
        public StochasticOscillator(int fastLength, int slowLength)
        {
            _fastLength = fastLength;
            _slowLength = slowLength;

            _values = new Queue<Candle>(_fastLength);
            _kValues = new Queue<decimal>(_fastLength);

            _movingAverage = new MovingAverage(slowLength);
        }

        public void Push(Candle candle)
        {
            if (_fastLength == 0) return;

            if (_values.Count == _fastLength)
            {
                _values.Dequeue();
            }
            _values.Enqueue(candle);

            if (_kValues.Count == _fastLength + _slowLength)
            {
                _kValues.Dequeue();
            }

            var highest = _values.Max(x => x.High);
            var lowest = _values.Min(x => x.Low);

            var kValue = highest == lowest ? 0 : (candle.Close - lowest) / (highest - lowest) * 100;
            _kValues.Enqueue(kValue);
            
            _movingAverage.Push(kValue);
            var dValue = _movingAverage.Value;

            KValue = kValue;
            DValue = dValue;
        }
    }
}
