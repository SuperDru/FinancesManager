using FinanceManagement.Indicators;
using NUnit.Framework;

namespace Test.Indicators
{
    public class ExponentialMovingAverageTest: FinanceTestBase
    {
        [Test]
        public void PositiveValues()
        {
            var ema = Indicators.RequireIndicator(new ExponentialMovingAverage(3), source: CandleSource.Close);
            Assert.AreEqual(0m, ema.Value);

            const decimal multiplier = 2m / (3m + 1m);
            
            const decimal expected1 = 1m;
            const decimal expected2 = 2m * multiplier + expected1 * (1 - multiplier);
            const decimal expected3 = 3m * multiplier + expected2 * (1 - multiplier);
            const decimal expected4 = 4m * multiplier + expected3 * (1 - multiplier);

            NewCandle(close: 1m);
            NewCandle(close: 2m);
            NewCandle(close: 3m);
            NewCandle(close: 4m);


            Assert.AreEqual(expected1, ema.Series[0]);
            Assert.AreEqual(expected2, ema.Series[1]);
            Assert.AreEqual(expected3, ema.Series[2]);
            Assert.AreEqual(expected4, ema.Series[3]);
        }
        
        [Test]
        public void NegativeValues()
        {
            var ema = Indicators.RequireIndicator(new ExponentialMovingAverage(3), source: CandleSource.Close);
            Assert.AreEqual(0m, ema.Value);

            const decimal multiplier = 2m / (3m + 1m);

            const decimal expected1 = 1m;
            const decimal expected2 = -2m * multiplier + expected1 * (1 - multiplier);
            const decimal expected3 = -3m * multiplier + expected2 * (1 - multiplier);
            const decimal expected4 = 4m * multiplier + expected3 * (1 - multiplier);
            
            NewCandle(close: 1m);
            NewCandle(close: -2m);
            NewCandle(close: -3m);
            NewCandle(close: 4m);
            
            Assert.AreEqual(expected1, ema.Series[0]);
            Assert.AreEqual(expected2, ema.Series[1]);
            Assert.AreEqual(expected3, ema.Series[2]);
            Assert.AreEqual(expected4, ema.Series[3]);
        }
        
        [Test]
        public void ZeroValues()
        {
            var ema = Indicators.RequireIndicator(new ExponentialMovingAverage(3), source: CandleSource.Close);
            
            NewCandle(close: 0m);
            NewCandle(close: 0m);
            NewCandle(close: 0m);

            Assert.AreEqual(0m, ema.Value);
            Assert.AreEqual(3m, ema.Series.Count);
        }
        
        [Test]
        public void AlwaysZeroWhenZeroLength()
        {
            var ema = Indicators.RequireIndicator(new ExponentialMovingAverage(0), source: CandleSource.Close);
            Assert.AreEqual(0m, ema.Value);
            Assert.AreEqual(0m, ema.Series.Count);

            NewCandle(close: 1m);
            Assert.AreEqual(0m, ema.Value);
            Assert.AreEqual(0m, ema.Series.Count);

            NewCandle(close: 0m);
            Assert.AreEqual(0m, ema.Value);
            Assert.AreEqual(0m, ema.Series.Count);
        }
    }
}