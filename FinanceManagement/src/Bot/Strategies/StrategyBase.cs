using System;
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
        
        public void Wait(decimal sellPrice)
        {
            SellPrice = sellPrice;
            PositionState = PositionState.Wait;
            AfterStopLossCandlesCount = 0;
        }
        
        public bool AddCandle(Candle candle)
        {
            Candles.AddCandle(candle);

            if (Candles.Minute.Count > 1 && Candles.Minute[^1].Time - Candles.Minute[^2].Time != TimeSpan.FromMinutes(1))
            {
                Logger.Warning($"Candles in inconsistent condition {Candles.Minute[^2].Time:g} -> {Candles.Minute[^1].Time:g}, resetting state");
                PositionState = PositionState.NotReady;
                BaseCandlesCount = 0;
                return false;
            }

            AfterStopLossCandlesCount++;
            BaseCandlesCount++;

            switch (PositionState)
            {
                case PositionState.NotReady when BaseCandlesCount == BaseCandlesCandlesConstraint:
                case PositionState.Wait when AfterStopLossCandlesCount > SkipStopLossCandlesConstraint:
                    PositionState = PositionState.Sold;
                    break;
            }

            return true;
        }
    }
    
    public abstract class StrategyBase: IStrategy
    {
        public abstract int BaseCandlesAmount { get; }
        
        protected State State { get; }

        private string Instrument { get; }

        private decimal? TakeProfit { get; }
        private decimal? StopLoss { get; }

        private int SkipCandlesAfterStopLoss { get; }

        protected StrategyBase(string instrument,  decimal? takeProfit, decimal? stopLoss, int skipAfterStopLoss = 3)
        {
            Instrument = instrument;
            TakeProfit = takeProfit / 100m;
            StopLoss = stopLoss / 100m;
            SkipCandlesAfterStopLoss = skipAfterStopLoss;
            
            State = new State
            {
                PositionState = PositionState.NotReady,
                SkipStopLossCandlesConstraint = SkipCandlesAfterStopLoss,
                // ReSharper disable once VirtualMemberCallInConstructor
                BaseCandlesCandlesConstraint = BaseCandlesAmount
            };
        }

        public StrategyAction ProcessTradingStep(Candle candle)
        {
            var lastState = State.PositionState;
            if (!State.AddCandle(candle) && lastState == PositionState.Bought)
            {
                return StrategyAction.Sell;
            }
            
            if (State.PositionState == PositionState.NotReady)
            {
                Logger.Info("It's too little candles, wait for market");
                return StrategyAction.NoTrade;
            }

            StrategyAction action;

            if (!ShouldProcess(out var info) || State.PositionState == PositionState.Wait) {
                if (State.PositionState is PositionState.Sold or PositionState.Wait){
                    action = StrategyAction.NoTrade;
                    Logger.Debug($"Instrument trading ignored {Instrument}, Candle: {candle.Close} {candle.Time:g}");
                }
                else
                {
                    State.Sell(candle.Close);
                    action = StrategyAction.Sell;
                    Logger.Info($"Sold because of bad trading conditions {Instrument}, Candle: {candle.Close} {candle.Time:g}");
                }

                OnProcessed(info);
                return action;
            }

            switch (State.PositionState)
            {
                case PositionState.Sold when ShouldBuy(info):
                    State.Buy(candle.Close);
                    action = StrategyAction.Buy;
                    Logger.Info($"Bought {Instrument}, Candle: {candle.Close} {candle.Time:g}, info: {info}");
                    break;
                case PositionState.Bought when ShouldSell(info):
                    State.Sell(candle.Close);
                    action = StrategyAction.Sell;
                    Logger.Info($"Sold {Instrument}, Candle: {candle.Close} {candle.Time:g}, info: {info}");
                    break;
                case PositionState.Bought when StopLoss != null && (State.BuyPrice - candle.Close) / State.BuyPrice >= StopLoss:
                    State.Wait(candle.Close);
                    action = StrategyAction.Sell;
                    Logger.Info($"Sold by Stop Loss {Instrument}, Candle: {candle.Close} {candle.Time:g}, info: {info}");
                    break;
                case PositionState.Bought when TakeProfit != null && (candle.Close - State.BuyPrice) / State.BuyPrice >= TakeProfit:
                    State.Sell(candle.Close);
                    action = StrategyAction.Sell;
                    Logger.Info($"Sold by Take Profit {Instrument}, Candle: {candle.Close} {candle.Time:g}, info: {info}");
                    break;
                default:
                    action = StrategyAction.NoTrade;
                    Logger.Debug($"No Trade {Instrument}, Candle: {candle.Close} {candle.Time:g}, info: {info}");
                    break;
            }

            OnProcessed(info);
            return action;
        }

        protected abstract bool ShouldSell(object info);
        protected abstract bool ShouldBuy(object info);
        protected abstract bool ShouldProcess(out object info);
        protected abstract void OnProcessed(object info);
    }
}