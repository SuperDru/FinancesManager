using System.Collections.Generic;
using System.Linq;

namespace FinanceManagement.Indicators
{
    public class MovingAverage: IIndicator
    {
        protected Queue<decimal> Values;
        protected int Length;

        private decimal _totalSum;

        public decimal Value => Values.Any() ? _totalSum / Values.Count : 0;

        public MovingAverage(int length)
        {
            Length = length;
            Values = new Queue<decimal>(length);
        }

        public void Push(decimal value)
        {
            if (Length == 0) return;

            _totalSum -= Values.Count == Length ? Values.Dequeue() : 0;

            _totalSum += value;
            Values.Enqueue(value);
        }
    }
}
