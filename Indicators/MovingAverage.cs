using System.Collections.Generic;
using System.Linq;

namespace FinancesManager.Indicators
{
    public class MovingAverage: IIndicator
    {
        protected Queue<decimal> Values;
        protected int Length;

        private decimal _totalSum;

        public decimal? Value => Values.Any() ? _totalSum / Values.Count : (decimal?) null;

        public MovingAverage(int length)
        {
            Length = length;
            Values = new Queue<decimal>(length);
        }

        public void Push(decimal value)
        {
            _totalSum -= Values.Count == Length ? Values.Dequeue() : 0;

            _totalSum += value;
            Values.Enqueue(value);
        }
    }
}
