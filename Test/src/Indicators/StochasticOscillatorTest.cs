using FinanceManagement.Common;
using FinanceManagement.Indicators;
using NUnit.Framework;

namespace Test.Indicators
{
    public class StochasticOscillatorTest: FinanceTestBase
    {
        [Test]
        public void FastLength()
        {
            var stoch = Indicators.RequireIndicator(new StochasticOscillator(3, 0));

            Assert.AreEqual(0m, stoch.KValue);
            Assert.AreEqual(0m, stoch.DValue);
            
            NewCandle(140, 135, 136, 130);
            const decimal expected1 = (135m - 130m) / (136m - 130m) * 100;
            Assert.AreEqual(expected1, stoch.KValue);
            Assert.AreEqual(0m, stoch.DValue);
            
            NewCandle(140, 140, 150, 135);
            const decimal expected2 = (140m - 130m) / (150m - 130m) * 100;
            Assert.AreEqual(expected2, stoch.KValue);
            Assert.AreEqual(0m, stoch.DValue);

            NewCandle(135, 145, 145, 125);
            const decimal expected3 = (145m - 125m) / (150m - 125m) * 100;
            Assert.AreEqual(expected3, stoch.KValue);
            Assert.AreEqual(0m, stoch.DValue);

            NewCandle(135, 140, 140, 130);
            const decimal expected4 = (140m - 125m) / (150m - 125m) * 100;
            Assert.AreEqual(expected4, stoch.KValue);
            Assert.AreEqual(0m, stoch.DValue);

            NewCandle(135, 120, 149, 110);
            const decimal expected5 = (120m - 110m) / (149m - 110m) * 100;
            Assert.AreEqual(expected5, stoch.KValue);
            Assert.AreEqual(0m, stoch.DValue);
        }
        
        [Test]
        public void FastLengthOnHourInterval()
        {
            var stoch = Indicators.RequireIndicator(new StochasticOscillator(3, 0), Interval.Hour);
            
            NewCandle(140, 135, 136, 130);
            const decimal expected1 = (135m - 130m) / (136m - 130m) * 100;
            Assert.AreEqual(expected1, stoch.KValue);
            Assert.AreEqual(0m, stoch.DValue);
            
            NewCandle(140, 140, 150, 135);
            const decimal expected2 = (140m - 130m) / (150m - 130m) * 100;
            Assert.AreEqual(expected2, stoch.KValue);
            Assert.AreEqual(0m, stoch.DValue);

            NewCandle(135, 145, 145, 125);
            const decimal expected3 = (145m - 125m) / (150m - 125m) * 100;
            Assert.AreEqual(expected3, stoch.KValue);
            Assert.AreEqual(0m, stoch.DValue);
        }
        
        [Test]
        public void SlowLength()
        {
            var stoch = Indicators.RequireIndicator(new StochasticOscillator(3, 2));

            NewCandle(140, 135, 136, 130);
            const decimal kValue1 = (135m - 130m) / (136m - 130m) * 100;
            Assert.AreEqual(kValue1, stoch.DValue);
            
            NewCandle(140, 140, 150, 135);
            const decimal kValue2 = (140m - 130m) / (150m - 130m) * 100;
            Assert.AreEqual((kValue1 + kValue2) / 2, stoch.DValue);

            NewCandle(135, 145, 145, 125);
            const decimal kValue3 = (145m - 125m) / (150m - 125m) * 100;
            Assert.AreEqual((kValue2 + kValue3) / 2, stoch.DValue);

            NewCandle(135, 140, 140, 130);
            const decimal kValue4 = (140m - 125m) / (150m - 125m) * 100;
            Assert.AreEqual((kValue3 + kValue4) / 2, stoch.DValue);

            NewCandle(135, 120, 149, 110);
            const decimal kValue5 = (120m - 110m) / (149m - 110m) * 100;
            Assert.AreEqual((kValue4 + kValue5) / 2, stoch.DValue);
        }

        [Test]
        public void ZeroWithZeroFastAndSlowLength()
        {
            var stoch = Indicators.RequireIndicator(new StochasticOscillator(0, 0));

            Assert.AreEqual(0m, stoch.KValue);
            Assert.AreEqual(0m, stoch.DValue);
            
            NewCandle(140, 135, 136, 130);
            Assert.AreEqual(0m, stoch.KValue);
            Assert.AreEqual(0m, stoch.DValue);
        }
    }
}