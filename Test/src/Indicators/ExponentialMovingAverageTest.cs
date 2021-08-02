using FinanceManagement.Indicators;
using NUnit.Framework;

namespace Test.Indicators
{
    [TestFixture]
    public class ExponentialMovingAverageTest
    {
        [Test]
        public void PositiveValues()
        {
            var ema = new ExponentialMovingAverage(3);
            Assert.AreEqual(0m, ema.Value);

            const decimal multiplier = 2m / (3m + 1m);

            ema.Push(1);
            const decimal expected1 = 1m;
            Assert.AreEqual(expected1, ema.Value);
            
            ema.Push(2);
            const decimal expected2 = 2m * multiplier + expected1 * (1 - multiplier);
            Assert.AreEqual(expected2, ema.Value);
            
            ema.Push(3);
            const decimal expected3 = 3m * multiplier + expected2 * (1 - multiplier);
            Assert.AreEqual(expected3, ema.Value);
            
            ema.Push(4);
            const decimal expected4 = 4m * multiplier + expected3 * (1 - multiplier);
            Assert.AreEqual(expected4, ema.Value);
        }
        
        [Test]
        public void NegativeValues()
        {
            var ema = new ExponentialMovingAverage(3);
            Assert.AreEqual(0m, ema.Value);

            const decimal multiplier = 2m / (3m + 1m);

            ema.Push(1);
            const decimal expected1 = 1m;
            Assert.AreEqual(expected1, ema.Value);
            
            ema.Push(-2);
            const decimal expected2 = -2m * multiplier + expected1 * (1 - multiplier);
            Assert.AreEqual(expected2, ema.Value);
            
            ema.Push(-3);
            const decimal expected3 = -3m * multiplier + expected2 * (1 - multiplier);
            Assert.AreEqual(expected3, ema.Value);
            
            ema.Push(4);
            const decimal expected4 = 4m * multiplier + expected3 * (1 - multiplier);
            Assert.AreEqual(expected4, ema.Value);
        }
        
        [Test]
        public void ZeroValues()
        {
            var ema = new ExponentialMovingAverage(3);
            
            ema.Push(0);
            ema.Push(0);
            ema.Push(0);

            Assert.AreEqual(0m, ema.Value);
        }
        
        [Test]
        public void AlwaysZeroWhenZeroLength()
        {
            var ema = new ExponentialMovingAverage(0);
            Assert.AreEqual(0, ema.Value);

            ema.Push(1);
            Assert.AreEqual(0, ema.Value);
            
            ema.Push(2);
            Assert.AreEqual(0, ema.Value);
        }
    }
}