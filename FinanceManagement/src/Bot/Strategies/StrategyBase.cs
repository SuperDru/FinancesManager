using System;
using FinanceManagement.Common;
using FinanceManagement.Indicators;

namespace FinanceManagement.Bot.Strategies
{
    public abstract class StrategyBase: IStrategy
    {
        public abstract int BaseCandlesAmount { get; }
        
        protected internal StrategyState State { get; }

        private string Instrument { get; }

        private decimal? TakeProfit { get; }
        private decimal? StopLoss { get; }

        private TimeSpan SkipCandlesAfterStopLoss { get; }

        protected IndicatorsCalculator Indicators { get; }

        protected StrategyBase(StrategyOptions options)
        {
            Instrument = options.Instrument;
            TakeProfit = options.TakeProfit / 100m;
            StopLoss = options.StopLoss / 100m;
            SkipCandlesAfterStopLoss = options.WaitAfterStopLoss ?? TimeSpan.FromMinutes(3);
            
            State = new StrategyState
            {
                PositionState = PositionState.NotReady,
                // ReSharper disable once VirtualMemberCallInConstructor
                BaseCandlesCandlesConstraint = BaseCandlesAmount
            };
            Indicators = new IndicatorsCalculator(State.Candles);
        }

        public StrategyAction ProcessTradingStep(Candle candle)
        {
            var lastState = State.PositionState;
            
            State.AddCandle(candle);
            
            var shouldProcess = OnProcessing(out var info);

            var action = State.PositionState switch
            {
                PositionState.NotReady when lastState == PositionState.Bought => SellByNotReady(),
                PositionState.NotReady => NotReady(),
                PositionState.Wait => UnsatisfyingMarket(candle),
                PositionState.Sold when !shouldProcess => UnsatisfyingMarket(candle),
                PositionState.Bought when !shouldProcess => SellByUnsatisfyingMarket(candle),
                PositionState.Sold when ShouldBuy(info) => BuyByStrategy(candle, info),
                PositionState.Bought when ShouldSell(info) => SellByStrategy(candle, info),
                PositionState.Bought when StopLoss != null && (State.BuyPrice - candle.Close) / State.BuyPrice >= StopLoss => SellByStopLoss(candle, info, SkipCandlesAfterStopLoss),
                PositionState.Bought when TakeProfit != null && (candle.Close - State.BuyPrice) / State.BuyPrice >= TakeProfit => SellByTakeProfit(candle, info),
                _ => NoEvents(candle, info)
            };

            OnProcessed(info);
            return action;
        }

        protected abstract bool OnProcessing(out object info);
        protected abstract bool ShouldSell(object info);
        protected abstract bool ShouldBuy(object info);
        protected abstract void OnProcessed(object info);

        protected virtual StrategyAction SellByNotReady()
        {
            Logger.Info("Sell because of not ready market");
            return StrategyAction.Sell;
        }

        protected virtual StrategyAction NotReady()
        {
            Logger.Info("It's too little candles, wait for the market to be ready");
            return StrategyAction.NoTrade;
        }
        
        protected virtual StrategyAction UnsatisfyingMarket(Candle candle)
        {
            Logger.Debug($"Instrument trading ignored {Instrument}, Candle: {candle.Close} {candle.Time:g}");
            return StrategyAction.NoTrade;
        }
        
        protected virtual StrategyAction SellByUnsatisfyingMarket(Candle candle)
        {
            Logger.Info($"Sold because of bad trading conditions {Instrument}, Candle: {candle.Close} {candle.Time:g}");
            State.Sell(candle.Close);
            return StrategyAction.Sell;
        }
        
        protected virtual StrategyAction BuyByStrategy(Candle candle, object info)
        {
            Logger.Info($"Bought {Instrument}, Candle: {candle.Close} {candle.Time:g}, info: {info}");
            State.Buy(candle.Close);
            return StrategyAction.Buy;
        }
        
        protected virtual StrategyAction SellByStrategy(Candle candle, object info)
        {
            Logger.Info($"Sold {Instrument}, Candle: {candle.Close} {candle.Time:g}, info: {info}");
            State.Sell(candle.Close);
            return StrategyAction.Sell;
        }
        
        protected virtual StrategyAction SellByStopLoss(Candle candle, object info, TimeSpan skipTimeSpan = default)
        {
            Logger.Info($"Sold by Stop Loss {Instrument}, Candle: {candle.Close} {candle.Time:g}, info: {info}");
            State.Sell(candle.Close);
            State.Wait(skipTimeSpan);
            return StrategyAction.Sell;
        }
        
        protected virtual StrategyAction SellByTakeProfit(Candle candle, object info)
        {
            Logger.Info($"Sold by Take Profit {Instrument}, Candle: {candle.Close} {candle.Time:g}, info: {info}");
            State.Sell(candle.Close);
            return StrategyAction.Sell;
        }
        
        protected virtual StrategyAction NoEvents(Candle candle, object info)
        {
            Logger.Debug($"No Trade {Instrument}, Candle: {candle.Close} {candle.Time:g}, info: {info}");
            return StrategyAction.NoTrade;
        }
    }
}