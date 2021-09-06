using System;

namespace FinanceManagement.Bot.Strategies
{
    public class StrategyOptions
    {
        public string Instrument { get; set; }
        public decimal? TakeProfit { get; set; }
        public decimal? StopLoss { get; set; }
        public TimeSpan? WaitAfterStopLoss { get; set; }
    }
}