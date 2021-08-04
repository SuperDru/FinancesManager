using System.Collections.Generic;
using FinanceManagement.Bot.Backtesting;
using FinanceManagement.Indicators;

namespace FinanceManagement.Common
{
    public class ComposeResult
    {
        public List<Candle> Candles { get; set; }
        public BacktestResult Backtest { get; set; }
        public Dictionary<string, List<IIndicator>> Indicators { get; set; }
    }
}