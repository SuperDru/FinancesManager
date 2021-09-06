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

    public class StrategyState
    {
        public CandlesStore Candles { get; } = new ();
        public PositionState PositionState { get; set; }
        public decimal BuyPrice { get; private set; } = -1;
        public decimal SellPrice { get; private set; } = -1;
        
        public int BaseCandlesCandlesConstraint { get; init; }
        private int BaseCandlesCount { get; set; }

        private Func<StrategyState, bool> ExitWaitCondition { get; set; }

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

            BaseCandlesCount++;

            switch (PositionState)
            {
                case PositionState.NotReady when BaseCandlesCount >= BaseCandlesCandlesConstraint:
                case PositionState.Wait when ExitWaitCondition(this):
                    PositionState = PositionState.Sold;
                    break;
            }
        }
    }
}