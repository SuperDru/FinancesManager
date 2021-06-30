namespace FinancesManager.Indicators
{
    public class ExponentialMovingAverage: IIndicator
    {
        private readonly decimal _multiplier;

        public decimal? Value { get; private set; }

        public ExponentialMovingAverage(int length)
        {
            _multiplier = 2 / (decimal) (length + 1);
        }

        public void Push(decimal value)
        {
            Value = Value != null 
                ? value * _multiplier + Value * (1 - _multiplier) 
                : value;
        }
    }
}
