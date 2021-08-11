using System;
using System.ComponentModel;
using System.Linq;
using FinanceManagement.Common;

namespace FinanceManagement.Bot.Strategies
{
    public enum PositionState
    {
        NotReady = 0,
        Bought = 1,
        Sold = 2,
        Wait = 3
    }

    public class State
    {
        public CandlesStore Candles { get; } = new ();
        public PositionState PositionState { get; set; }
        public decimal BuyPrice { get; set; } = -1;
        public decimal SellPrice { get; set; } = -1;
        
        public int SkipStopLossCandlesConstraint { get; set; }
        public int BaseCandlesCandlesConstraint { get; set; }
        
        private int AfterStopLossCandlesCount { get; set; }
        private int BaseCandlesCount { get; set; }

        private Func<State, bool> ExitWaitCondition { get; set; }

        public void Buy(decimal buyPrice)
        {
            BuyPrice = buyPrice;
            PositionState = PositionState.Bought;
        }

        public void Sell(decimal sellPrice)
        {
            SellPrice = sellPrice;
            PositionState = PositionState.Sold;
        }
        
        public void SellByStopLoss(decimal sellPrice)
        {
            SellPrice = sellPrice;
            PositionState = PositionState.Wait;
            AfterStopLossCandlesCount = 0;
        }
        
        public void Wait(TimeSpan time)
        {
            PositionState = PositionState.Wait;
            var lastTime = Candles.Time;
            ExitWaitCondition = state => state.Candles.Time > lastTime + time;
        }
        
        public void AddCandle(Candle candle)
        {
            var added = Candles.AddCandle(candle);
            if (!added)
            {
                Logger.Warning($"Expected latest candle {Candles.Minute[^1].Time:g} -> {candle.Time:g}");
                PositionState = PositionState.NotReady;
                return;
            }

            if (Candles.Minute.Count > 1 && Candles.Minute[^1].Time - Candles.Minute[^2].Time != TimeSpan.FromMinutes(1))
            {
                Logger.Warning($"Candles in inconsistent condition {Candles.Minute[^2].Time:g} -> {Candles.Minute[^1].Time:g}, resetting state");
                PositionState = PositionState.NotReady;
                BaseCandlesCount = 0; // reset state to wait again for new base candles
            }

            AfterStopLossCandlesCount++;
            BaseCandlesCount++;

            switch (PositionState)
            {
                case PositionState.NotReady when BaseCandlesCount >= BaseCandlesCandlesConstraint:
                // case PositionState.Wait when AfterStopLossCandlesCount > SkipStopLossCandlesConstraint:
                case PositionState.Wait when ExitWaitCondition(this):
                    PositionState = PositionState.Sold;
                    break;
            }
        }
    }
    
    public abstract class StrategyBase: IStrategy
    {
        public abstract int BaseCandlesAmount { get; }
        
        protected internal State State { get; }

        private string Instrument { get; }

        private decimal? TakeProfit { get; }
        private decimal? StopLoss { get; }

        private TimeSpan SkipCandlesAfterStopLoss { get; }

        protected StrategyBase(string instrument,  decimal? takeProfit = null, decimal? stopLoss = null, TimeSpan? waitAfterStopLoss = null)
        {
            Instrument = instrument;
            TakeProfit = takeProfit / 100m;
            StopLoss = stopLoss / 100m;
            SkipCandlesAfterStopLoss = waitAfterStopLoss ?? TimeSpan.FromMinutes(3);
            
            State = new State
            {
                PositionState = PositionState.NotReady,
                // ReSharper disable once VirtualMemberCallInConstructor
                BaseCandlesCandlesConstraint = BaseCandlesAmount
            };
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