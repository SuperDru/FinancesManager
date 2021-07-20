using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinanceManagement.Common;

namespace FinanceManagement.Bot.Backtesting
{
    public class Backtest
    {
        private string Instrument { get; }
        private IStrategy Strategy { get; }
        
        private BacktestContext Context { get; set; }
        
        public Backtest(string instrument, IStrategy strategy)
        {
            Strategy = strategy;
            Instrument = instrument;
        }

        public async Task<StrategyStatistics> Process(IFinanceDataApi api, DateTime? from = null, DateTime? to = null)
        {
            var candles = await api.GetCandlesAsync(Instrument, Interval.Minute, from, to);
            return Process(candles);
        }
        
        public StrategyStatistics Process(List<Candle> candles)
        {
            var baseCandles = candles.Take(Strategy.BaseCandlesAmount).ToList();

            if (baseCandles.Count < Strategy.BaseCandlesAmount)
            {
                var e = new Exception("Not enough candles for strategy processing");
                Logger.Error("Backtesting has failed", e);
                throw e;
            }

            Context = new BacktestContext();
            Strategy.Init(baseCandles);
            foreach (var candle in candles.Skip(Strategy.BaseCandlesAmount))
            {
                var action = Strategy.ProcessTradingStep(candle);
                ProcessStrategyResponse(action, candle);
            }
            
            Strategy.Reset();

            return Context.Statistics;
        }

        private void ProcessStrategyResponse(StrategyResult action, Candle candle)
        {
            Context ??= new BacktestContext();
            switch (action)
            {
                case StrategyResult.Buy:
                    Context.Buy(candle);
                    break;
                case StrategyResult.Sell:
                    Context.Sell(candle);
                    Logger.Info($"Close position {Instrument}. {Context.Statistics}");
                    break;
                case StrategyResult.NoTrade:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action));
            }
        }
    }
}