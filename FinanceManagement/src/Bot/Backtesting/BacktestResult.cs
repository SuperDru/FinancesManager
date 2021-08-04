using System.Collections.Generic;

namespace FinanceManagement.Bot.Backtesting
{
    public class BacktestResult
    {
        public List<Position> Positions { get; set; }
        public List<BalanceSnapshot> BalanceSnapshots { get; set; }
        public StrategyStatistics Statistics { get; set; }

        public override string ToString() => Statistics.ToString();
    }
}