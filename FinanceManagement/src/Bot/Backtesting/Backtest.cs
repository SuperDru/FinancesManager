using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinanceManagement.Bot.Strategies;
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

        public async Task<BacktestResult> Process(IFinanceDataApi api, DateTime? from = null, DateTime? to = null)
        {
            var candles = await api.GetCandlesAsync(Instrument, Interval.Minute, from, to);
            return Process(candles);
        }
        
        public BacktestResult Process(List<Candle> candles)
        {
            Init(candles);

            foreach (var candle in candles.Skip(Strategy.BaseCandlesAmount))
            {
                var action = Strategy.ProcessTradingStep(candle);
                ProcessStrategyAction(action, candle);
            }

            return new BacktestResult
            {
                Positions = Context.Positions.Where(_ => _.Closed).ToList(),
                BalanceSnapshots = Context.BalanceSnapshots,
                Statistics = Context.Statistics
            };
        }

        private void ProcessStrategyAction(StrategyAction action, Candle candle)
        {
            Context ??= new BacktestContext();
            switch (action)
            {
                case StrategyAction.Buy:
                    Context.Buy(candle);
                    break;
                case StrategyAction.Sell:
                    Context.Sell(candle);
                    Logger.Info($"Close position {Instrument}. {Context.Statistics}");
                    break;
                case StrategyAction.NoTrade:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action));
            }
        }

        private void Init(IEnumerable<Candle> candles)
        {
            var baseCandles = candles.Take(Strategy.BaseCandlesAmount).ToList();

            if (baseCandles.Count < Strategy.BaseCandlesAmount)
            {
                var e = new Exception("Not enough candles for strategy processing");
                Logger.Error("Backtesting has failed", e);
                throw e;
            }

            Context = new BacktestContext();
        }
    }
}