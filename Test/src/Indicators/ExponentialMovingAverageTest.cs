using System.Linq;
using FinanceManagement.Indicators;
using NUnit.Framework;

namespace Test.Indicators
{
    public class ExponentialMovingAverageTest
    {
        [Test]
        public void PositiveValues()
        {
            var ema = new ExponentialMovingAverage(3);
            Assert.AreEqual(0m, ema.Value);

            const decimal multiplier = 2m / (3m + 1m);
            
            const decimal expected1 = 1m;
            const decimal expected2 = 2m * multiplier + expected1 * (1 - multiplier);
            const decimal expected3 = 3m * multiplier + expected2 * (1 - multiplier);
            const decimal expected4 = 4m * multiplier + expected3 * (1 - multiplier);

            ema.Push(1);
            ema.Push(2);
            ema.Push(3);
            ema.Push(4);

            Assert.AreEqual(expected1, ema.Series[0]);
            Assert.AreEqual(expected2, ema.Series[1]);
            Assert.AreEqual(expected3, ema.Series[2]);
            Assert.AreEqual(expected4, ema.Series[3]);
        }
        
        [Test]
        public void NegativeValues()
        {
            var ema = new ExponentialMovingAverage(3);
            Assert.AreEqual(0m, ema.Value);

            const decimal multiplier = 2m / (3m + 1m);

            const decimal expected1 = 1m;
            const decimal expected2 = -2m * multiplier + expected1 * (1 - multiplier);
            const decimal expected3 = -3m * multiplier + expected2 * (1 - multiplier);
            const decimal expected4 = 4m * multiplier + expected3 * (1 - multiplier);
            
            ema.Push(1);
            ema.Push(-2);
            ema.Push(-3);
            ema.Push(4);
            
            Assert.AreEqual(expected1, ema.Series[0]);
            Assert.AreEqual(expected2, ema.Series[1]);
            Assert.AreEqual(expected3, ema.Series[2]);
            Assert.AreEqual(expected4, ema.Series[3]);
        }
        
        [Test]
        public void ZeroValues()
        {
            var ema = new ExponentialMovingAverage(3);
            
            ema.Push(0);
            ema.Push(0);
            ema.Push(0);

            Assert.AreEqual(0m, ema.Value);
            Assert.AreEqual(3, ema.Series.Count);
        }
        
        [Test]
        public void AlwaysZeroWhenZeroLength()
        {
            var ema = new ExponentialMovingAverage(0);
            Assert.AreEqual(0, ema.Value);
            Assert.AreEqual(0, ema.Series.Count);

            ema.Push(1);
            Assert.AreEqual(0, ema.Value);
            Assert.AreEqual(0, ema.Series.Count);

            ema.Push(2);
            Assert.AreEqual(0, ema.Value);
            Assert.AreEqual(0, ema.Series.Count);
        }
    }
}