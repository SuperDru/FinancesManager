using System;
using System.Linq;
using FinanceManagement.Common;
using FinanceManagement.Indicators;
using NUnit.Framework;

namespace Test
{
    [TestFixture]
    public class FinanceTestBase
    {
        protected CandlesStore Store { get; private set; }
        protected IndicatorsCalculator Indicators { get; private set; }

        [SetUp]
        public void SetUp()
        {
            Store = new CandlesStore();
            Indicators = new IndicatorsCalculator(Store);
        }
        
        protected void NewCandle(
            decimal open = 2500m,
            decimal close = 2550,
            decimal high = 2600m,
            decimal low = 2450m)
        {
            var candles = Store.Minute;
            var time = candles.LastOrDefault()?.Time.AddMinutes(1) ??
                       new DateTime(2021, 1, 1, 1, 0, 0);
            NewCandle(time, open, close, high, low);
        }
        
        protected void NewCandle(
            DateTime time,
            decimal open = 2500m,
            decimal close = 2550,
            decimal high = 2600m,
            decimal low = 2450m)
        {
            Store.AddCandle(new Candle
            {
                Interval = Interval.Minute,
                Time = time,
                Open = open,
                Close = close,
                High = high,
                Low = low
            });
        }
    }
}