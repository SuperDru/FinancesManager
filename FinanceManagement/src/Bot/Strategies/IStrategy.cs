using FinanceManagement.Common;

namespace FinanceManagement.Bot.Strategies
{
    public enum StrategyAction
    {
        Buy = 1,
        Sell = 2,
        NoTrade = 3
    }
    
    public interface IStrategy
    {
        public int BaseCandlesAmount { get; }
        StrategyAction ProcessTradingStep(Candle candle);
    }
}