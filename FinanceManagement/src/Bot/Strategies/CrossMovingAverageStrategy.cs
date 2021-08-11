using FinanceManagement.Bot.Strategies;

namespace FinanceManagement.Bot
{
    public class CrossMovingAverageStrategy: StrategyBase
    {
        public CrossMovingAverageStrategy(string instrument, decimal? takeProfit, decimal? stopLoss) : base(instrument, takeProfit, stopLoss)
        {
        }

        public override int BaseCandlesAmount { get; }
        protected override bool ShouldSell(object info)
        {
            throw new System.NotImplementedException();
        }

        protected override bool ShouldBuy(object info)
        {
            throw new System.NotImplementedException();
        }

        protected override bool OnProcessing(out object info)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnProcessed(object info)
        {
            throw new System.NotImplementedException();
        }
    }
}