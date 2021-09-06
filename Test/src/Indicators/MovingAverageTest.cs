using FinanceManagement.Indicators;
using NUnit.Framework;

namespace Test.Indicators
{
    public class MovingAverageTest: FinanceTestBase
    {
        [Test]
        public void SingleValue()
        {
            var ma = Indicators.RequireIndicator(new MovingAverage(1), source: CandleSource.Close);
            
            NewCandle(close: 1);

            Assert.AreEqual(1, ma.Value); // 1 / 1
            Assert.AreEqual(1, ma.Series[0]);
        }
        
        [Test]
        public void MultipleValues()
        {
            var ma = Indicators.RequireIndicator(new MovingAverage(3), source: CandleSource.Close);
            
            NewCandle(close: 1);
            NewCandle(close: 2);
            NewCandle(close: 3);

            Assert.AreEqual(2, ma.Value); // (1 + 2 + 3) / 3
            Assert.AreEqual(1, ma.Series[0]);
            Assert.AreEqual(1.5, ma.Series[1]);
            Assert.AreEqual(2, ma.Series[2]);
        }
        
        [Test]
        public void LessThanLength()
        {
            var ma = Indicators.RequireIndicator(new MovingAverage(3), source: CandleSource.Close);
            
            NewCandle(close: 4);
            NewCandle(close: 2);

            Assert.AreEqual(3, ma.Value); // (4 + 2) / 2
            Assert.AreEqual(4, ma.Series[0]);
            Assert.AreEqual(3, ma.Series[1]);
        }
        
        [Test]
        public void MoreThanLength()
        {
            var ma = Indicators.RequireIndicator(new MovingAverage(3), source: CandleSource.Close);
            
            NewCandle(close: 5);
            NewCandle(close: 2);
            NewCandle(close: 1);
            NewCandle(close: 3);
            
            Assert.AreEqual(2, ma.Value); // (2 + 1 + 3) / 3
            Assert.AreEqual(5, ma.Series[0]);
            Assert.AreEqual(3.5m, ma.Series[1]);
            Assert.AreEqual(8m / 3m, ma.Series[2]);
            Assert.AreEqual(2m, ma.Series[3]);
            
        }
        
        [Test]
        public void ZeroValues()
        {
            var ma = Indicators.RequireIndicator(new MovingAverage(3), source: CandleSource.Close);
            
            NewCandle(close: 0);
            NewCandle(close: 0);
            NewCandle(close: 0);

            Assert.AreEqual(0, ma.Value); // (0 + 0 + 0) / 3
            Assert.AreEqual(0, ma.Series[0]);
            Assert.AreEqual(0, ma.Series[1]);
            Assert.AreEqual(0, ma.Series[2]);
        }
        
        [Test]
        public void NegativeValues()
        {
            var ma = Indicators.RequireIndicator(new MovingAverage(3), source: CandleSource.Close);
            
            NewCandle(close: -1);
            NewCandle(close: -2);
            NewCandle(close: -3);

            Assert.AreEqual(-2, ma.Value); // (-1 + -2 + -3) / 3
            Assert.AreEqual(-1, ma.Series[0]);
            Assert.AreEqual(-1.5, ma.Series[1]);
            Assert.AreEqual(-2, ma.Series[2]);
        }
        
        [Test]
        public void ZeroWithoutValues()
        {
            var ma = Indicators.RequireIndicator(new MovingAverage(3), source: CandleSource.Close);

            Assert.AreEqual(0, ma.Value);
            Assert.AreEqual(0, ma.Series.Count);
        }
        
        [Test]
        public void AlwaysZeroWhenZeroLength()
        {
            var ma = Indicators.RequireIndicator(new MovingAverage(0), source: CandleSource.Close);
            Assert.AreEqual(0, ma.Value);

            NewCandle(close: 1);
            Assert.AreEqual(0, ma.Value);
            
            NewCandle(close: 2);
            Assert.AreEqual(0, ma.Value);
            
            Assert.AreEqual(0, ma.Series.Count);
        }
    }
}