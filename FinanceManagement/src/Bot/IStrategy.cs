using System.Collections.Generic;
using System.Linq;
using FinanceManagement.Common;

namespace FinanceManagement.Bot
{
    public enum StrategyAction
    {
        Buy = 1,
        Sell = 2,
        NoTrade = 3
    }

    public class StrategyResponse
    {
        public StrategyAction Action { get; set; }
        public Dictionary<decimal, StrategyAction> Triggers { get; set; }

        public static StrategyResponse Create(StrategyAction action,
            params (decimal? TriggerPrice, StrategyAction TriggerAction)[] triggers) =>
            new()
            {
                Action = action,
                Triggers = triggers
                    .Where(_ => _.TriggerPrice != null)
                    .ToDictionary(_ => _.TriggerPrice.Value, _ => _.TriggerAction)
            };
    }
    
    public interface IStrategy
    {
        public int BaseCandlesAmount { get; }

        void Reset();
        void Init(List<Candle> candles);
        StrategyAction ProcessTradingStep(Candle candle);
    }
}