using FinanceManagement.Indicators;

namespace FinanceManagement.Bot
{
    public sealed class StochLessMoreStrategy: StrategyBase
    {
        public override int BaseCandlesAmount => 17; // 14 + 3

        private decimal Overbought { get; }
        private decimal Oversold { get; }
        private decimal Threshold { get; }
        
        private decimal LastHigh { get; set; }
        private decimal LastLow { get; set; }

        public StochLessMoreStrategy(string instrument,
            decimal overbought, decimal oversold, decimal threshold, 
            decimal? takeProfit = null, decimal? stopLoss = null): base(instrument, takeProfit, stopLoss)
        {
            Overbought = overbought;
            Oversold = oversold;
            Threshold = threshold;
            
            Reset();
        }
        
        public override void Reset()
        {
            base.Reset();
            LastHigh = 0;
            LastLow = 100;
        }

        protected override bool ShouldSell(object info) => LastHigh - (decimal) info > Threshold;

        protected override bool ShouldBuy(object info) => (decimal) info - LastLow > Threshold;

        protected override object CalculateSpecificInfo() => CandlesStorage.MinuteCandles.STOCH();

        protected override void OnProcessed(object info) => AddStochValue((decimal) info);

        private void AddStochValue(decimal stoch)
        {
            LastHigh = stoch > LastHigh && stoch > Overbought ? stoch : stoch > Overbought ? LastHigh : 0;
            LastLow = stoch < LastLow && stoch < Oversold ? stoch : stoch < Oversold ? LastLow : 100;
        }
    }
}
