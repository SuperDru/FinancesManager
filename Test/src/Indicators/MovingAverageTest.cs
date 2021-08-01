using FinanceManagement.Indicators;
using NUnit.Framework;

namespace Test.Indicators
{
    [TestFixture]
    public class MovingAverageTest
    {
        [Test]
        public void SingleValue()
        {
            var ma = new MovingAverage(1);
            
            ma.Push(1);

            Assert.AreEqual(1, ma.Value); // 1 / 1
        }
        
        [Test]
        public void MultipleValues()
        {
            var ma = new MovingAverage(3);
            
            ma.Push(1);
            ma.Push(2);
            ma.Push(3);

            Assert.AreEqual(2, ma.Value); // (1 + 2 + 3) / 3
        }
        
        [Test]
        public void LessThanLength()
        {
            var ma = new MovingAverage(3);
            
            ma.Push(4);
            ma.Push(2);

            Assert.AreEqual(3, ma.Value); // (4 + 2) / 2
        }
        
        [Test]
        public void MoreThanLength()
        {
            var ma = new MovingAverage(3);
            
            ma.Push(5);
            ma.Push(2);
            ma.Push(1);
            ma.Push(3);
            
            Assert.AreEqual(2, ma.Value); // (2 + 1 + 3) / 3
        }
        
        [Test]
        public void ZeroValues()
        {
            var ma = new MovingAverage(3);
            
            ma.Push(0);
            ma.Push(0);
            ma.Push(0);

            Assert.AreEqual(0, ma.Value); // (0 + 0 + 0) / 3
        }
        
        [Test]
        public void NegativeValues()
        {
            var ma = new MovingAverage(3);
            
            ma.Push(-1);
            ma.Push(-2);
            ma.Push(-3);

            Assert.AreEqual(-2, ma.Value); // (-1 + -2 + -3) / 3
        }
        
        [Test]
        public void ZeroWithoutValues()
        {
            var ma = new MovingAverage(3);

            Assert.AreEqual(0, ma.Value);
        }
        
        [Test]
        public void AlwaysZeroWhenZeroLength()
        {
            var ma = new MovingAverage(0);
            Assert.AreEqual(0, ma.Value);

            ma.Push(1);
            Assert.AreEqual(0, ma.Value);
            
            ma.Push(2);
            Assert.AreEqual(0, ma.Value);
        }
    }
}