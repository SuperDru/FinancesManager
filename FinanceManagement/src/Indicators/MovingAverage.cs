using System.Collections.Generic;
using System.Linq;

namespace FinanceManagement.Indicators
{
    public class MovingAverage: IIndicator
    {
        private readonly Queue<decimal> _values;
        private readonly int _length;

        private decimal _totalSum;

        public decimal Value => _values.Any() ? _totalSum / _values.Count : 0;

        public MovingAverage(int length)
        {
            _length = length;
            _values = new Queue<decimal>(length);
        }

        public void Push(decimal value)
        {
            if (_length == 0) return;

            _totalSum -= _values.Count == _length ? _values.Dequeue() : 0;

            _totalSum += value;
            _values.Enqueue(value);
        }
    }
}
