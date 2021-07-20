using System.Collections.Generic;
using FinanceManagement.Common;

namespace FinanceManagement.Bot
{
    public enum StrategyResult
    {
        Buy = 1,
        Sell = 2,
        NoTrade = 3
    }
    
    public interface IStrategy
    {
        public int BaseCandlesAmount { get; }

        void Reset();
        void Init(List<Candle> candles);
        StrategyResult ProcessTradingStep(Candle candle);
    }
}