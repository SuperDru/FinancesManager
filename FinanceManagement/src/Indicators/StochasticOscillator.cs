using System.Collections.Generic;
using System.Linq;
using Tinkoff.Trading.OpenApi.Models;

namespace FinanceManagement.Indicators
{
    public class StochasticOscillator
    {
        private readonly Queue<CandlePayload> _values;
        private readonly Queue<decimal> _kValues;

        private readonly int _fastLength;
        private readonly int _slowLength;

        public decimal? KValue;
        public decimal? DValue;

        public StochasticOscillator(int fastLength, int slowLength)
        {
            _fastLength = fastLength;
            _slowLength = slowLength;

            _values = new Queue<CandlePayload>(_fastLength);
            _kValues = new Queue<decimal>(_fastLength);
        }

        public void Push(CandlePayload candle)
        {
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

            var dValue = _kValues.MA(_slowLength);

            KValue = kValue;
            DValue = dValue;
        }
    }
}
