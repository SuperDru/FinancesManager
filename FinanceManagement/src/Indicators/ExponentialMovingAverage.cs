using System.Collections.Generic;
using FinanceManagement.Common;

namespace FinanceManagement.Indicators
{
    public class ExponentialMovingAverage: IIndicator
    {
        private readonly decimal _multiplier;
        private readonly decimal _length;

        private decimal? _value;
        public decimal Value => _value ?? 0m;
        
        public List<object> Series { get; } = new();

        public ExponentialMovingAverage(int length)
        {
            _length = length;
            _multiplier = 2 / (decimal) (length + 1);
        }

        public void Push(Candle candle) => Push(candle.Close);

        public void Push(decimal value)
        {
            if (_length == 0m) return;

            _value = _value != null 
                ? value * _multiplier + Value * (1 - _multiplier) 
                : value;
            
            Series.Add(_value);
        }
    }
}
