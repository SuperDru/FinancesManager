using System;

namespace FinanceManagement.Bot.Backtesting
{
    public class BalanceSnapshot
    {
        public DateTime Time { get; set; }
        public decimal Balance { get; set; }
    }
}