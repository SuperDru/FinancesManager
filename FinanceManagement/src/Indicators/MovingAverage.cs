using System.Collections.Generic;
using System.Linq;
using FinanceManagement.Common;

namespace FinanceManagement.Indicators
{
    public class MovingAverage: IIndicator
    {
        private readonly Queue<decimal> _values;
        private readonly int _length;

        private decimal _totalSum;

        public decimal Value => (decimal) (Series.LastOrDefault() ?? 0m);
        public List<object> Series { get; } = new();

        public MovingAverage(int length)
        {
            _length = length;
            _values = new Queue<decimal>(length);
        }

        public void Push(Candle candle) => Push(candle.Close);

        public void Push(decimal value)
        {
            if (_length == 0) return;

            _totalSum -= _values.Count == _length ? _values.Dequeue() : 0;
            _totalSum += value;
            
            _values.Enqueue(value);
            
            Series.Add(_totalSum / _values.Count);
        }
    }
}
