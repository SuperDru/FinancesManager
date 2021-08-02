namespace FinanceManagement.Indicators
{
    public class ExponentialMovingAverage: IIndicator
    {
        private readonly decimal _multiplier;
        private readonly decimal _length;

        private decimal? _value;
        public decimal Value => _value ?? 0m;

        public ExponentialMovingAverage(int length)
        {
            _length = length;
            _multiplier = 2 / (decimal) (length + 1);
        }

        public void Push(decimal value)
        {
            if (_length == 0m) return;

            _value = _value != null 
                ? value * _multiplier + Value * (1 - _multiplier) 
                : value;
        }
    }
}
