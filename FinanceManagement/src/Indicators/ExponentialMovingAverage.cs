using System.Collections.Generic;

namespace FinanceManagement.Indicators
{
    public class ExponentialMovingAverage: IDecimalIndicator
    {
        public List<object> Series { get; } = new();

        private readonly decimal _multiplier;
        private readonly decimal _length;

        private decimal? _value;
        public decimal Value => _value ?? 0m;

        public ExponentialMovingAverage(int length)
        {
            _length = length;
            _multiplier = 2 / (decimal) (length + 1);
        }

        private decimal _lastValue;
        public void Push(decimal value, bool changed = false)
        {
            if (_length == 0m) return;

            if (changed)
            {
                _value = _lastValue;
            }
            _value = _value != null
                ? value * _multiplier + Value * (1 - _multiplier)
                : value;

            _lastValue = value;
            Series.Add(_value);
        }
    }
}
