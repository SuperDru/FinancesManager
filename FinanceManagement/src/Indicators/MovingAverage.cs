using System.Collections.Generic;
using System.Linq;

namespace FinanceManagement.Indicators
{
    public class MovingAverage: IDecimalIndicator
    {
        public List<object> Series { get; } = new();

        private LinkedList<decimal> _values;
        private int _length;

        private decimal _totalSum;

        public decimal Value => (decimal) (Series.LastOrDefault() ?? 0m);

        public MovingAverage(int length)
        {
            _length = length;
            _values = new LinkedList<decimal>();
        }
        
        public void Push(decimal value, bool changed = false)
        {
            if (_length == 0) return;

            if (changed && _values.Count >= 1)
            {
                _totalSum -= _values.Last();
                _values.RemoveLast();
            }
            else if (_values.Count == _length)
            {
                _totalSum -= _values.First();
                _values.RemoveFirst();
            }
            _totalSum += value;
            _values.AddLast(value);

            Series.Add(_totalSum / _values.Count);
        }
    }
}
