using FinanceManagement.Common;
using FinanceManagement.Indicators;

namespace FinanceManagement.Bot.Strategies
{
    public class StochLessMoreStrategyOptions: StrategyOptions
    {
        public decimal Overbought { get; set; }
        public decimal Oversold { get; set; }
        public decimal Threshold { get; set; }
    }
    
    public class StochLessMoreStrategy: StrategyBase
    {
        public override int BaseCandlesAmount => 17; // 14 + 3

        private decimal Overbought { get; }
        private decimal Oversold { get; }
        private decimal Threshold { get; }
        
        private decimal LastHigh { get; set; }
        private decimal LastLow { get; set; }

        private StochasticOscillator StochasticOscillator { get; } 
        private MovingAverage MovingAverage50Hours { get; } 
        
        public StochLessMoreStrategy(StochLessMoreStrategyOptions options) : base(options)
        {
            Overbought = options.Overbought;
            Oversold = options.Oversold;
            Threshold = options.Threshold;
            
            LastHigh = 0;
            LastLow = 100;

            StochasticOscillator = Indicators.RequireIndicator(new StochasticOscillator(14, 3));
            MovingAverage50Hours = Indicators.RequireIndicator(new MovingAverage(50), Interval.Hour, CandleSource.Close);
        }
        
        protected override bool ShouldSell(object info) => LastHigh - (decimal) info > Threshold;

        protected override bool ShouldBuy(object info) => (decimal) info - LastLow > Threshold;

        protected override bool OnProcessing(out object info)
        {
            info = StochasticOscillator.DValue;
            var lastCandle = State.Candles.Minute[^1];
            return lastCandle.Close > MovingAverage50Hours.Value;
        }

        protected override void OnProcessed(object info)
        {
            var stoch = (decimal) info;
            LastHigh = stoch > LastHigh && stoch > Overbought ? stoch : stoch > Overbought ? LastHigh : 0;
            LastLow = stoch < LastLow && stoch < Oversold ? stoch : stoch < Oversold ? LastLow : 100;
        }
    }
}
