using FinanceManagement.Common;
using FinanceManagement.Indicators;
using NUnit.Framework;

namespace Test.Indicators
{
    [TestFixture]
    public class StochasticOscillatorTest
    {
        [Test]
        public void FastLength()
        {
            var stoch = new StochasticOscillator(3, 0);
            
            stoch.Push(new Candle
            {
                Open = 140,
                Close = 135,
                High = 136,
                Low = 130
            });

            const decimal expected1 = (135m - 130m) / (136m - 130m) * 100;
            Assert.AreEqual(expected1, stoch.KValue);
            
            stoch.Push(new Candle
            {
                Open = 140,
                Close = 140,
                High = 150,
                Low = 135
            });

            const decimal expected2 = (140m - 130m) / (150m - 130m) * 100;
            Assert.AreEqual(expected2, stoch.KValue);
            
            stoch.Push(new Candle
            {
                Open = 135,
                Close = 145,
                High = 145,
                Low = 125
            });

            const decimal expected3 = (145m - 125m) / (150m - 125m) * 100;
            Assert.AreEqual(expected3, stoch.KValue);
            
            stoch.Push(new Candle
            {
                Open = 135,
                Close = 140,
                High = 140,
                Low = 130
            });

            const decimal expected4 = (140m - 125m) / (150m - 125m) * 100;
            Assert.AreEqual(expected4, stoch.KValue);
            
            stoch.Push(new Candle
            {
                Open = 135,
                Close = 120,
                High = 149,
                Low = 110
            });

            const decimal expected5 = (120m - 110m) / (149m - 110m) * 100;
            Assert.AreEqual(expected5, stoch.KValue);
        }
    }
}