using System.Linq;
using FinanceManagement.Indicators;

namespace FinanceManagement.Bot.Strategies
{
    public sealed class StochLessMoreStrategy: StrategyBase
    {
        public override int BaseCandlesAmount => 17; // 14 + 3

        private decimal Overbought { get; }
        private decimal Oversold { get; }
        private decimal Threshold { get; }
        
        private decimal LastHigh { get; set; }
        private decimal LastLow { get; set; }

        private StochasticOscillator StochasticOscillator { get; } 
        private MovingAverage MovingAverage50Hours { get; } 
        
        public StochLessMoreStrategy(string instrument,
            decimal overbought, decimal oversold, decimal threshold, 
            decimal? takeProfit = null, decimal? stopLoss = null)
            : base(instrument, takeProfit, stopLoss)
        {
            Overbought = overbought;
            Oversold = oversold;
            Threshold = threshold;
            
            LastHigh = 0;
            LastLow = 100;
            StochasticOscillator = new StochasticOscillator(14, 3);
            MovingAverage50Hours = new MovingAverage(50);
        }
        
        protected override bool ShouldSell(object info) => LastHigh - (decimal) info > Threshold;

        protected override bool ShouldBuy(object info) => (decimal) info - LastLow > Threshold;

        protected override bool ShouldProcess(out object info)
        {
            var lastCandle = State.Candles.Minute[^1];
            MovingAverage50Hours.Push(lastCandle.Close);
            StochasticOscillator.Push(lastCandle);
            info = StochasticOscillator.DValue;
            
            // return lastCandle.Close > MovingAverage50Hours.Value;
            return true;
        }

        protected override void OnProcessed(object info)
        {
            var stoch = (decimal) info;
            LastHigh = stoch > LastHigh && stoch > Overbought ? stoch : stoch > Overbought ? LastHigh : 0;
            LastLow = stoch < LastLow && stoch < Oversold ? stoch : stoch < Oversold ? LastLow : 100;
        }
    }
}
