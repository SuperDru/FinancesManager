using System;
using System.Collections.Generic;
using System.Linq;
using FinanceManagement.Common;
using FinanceManagement.Indicators;
using Tinkoff.Trading.OpenApi.Models;

namespace FinanceManagement.Bot
{
    public enum PositionState
    {
        Bought = 1,
        Sold = 2
    }

    public class State
    {
        public PositionState PositionState { get; set; }
        public decimal BuyPrice { get; set; }
        public decimal SellPrice { get; set; }

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
    }

    
    public class StochLessMoreStrategy: IStrategy
    {
        public int BaseCandlesAmount => 17; // 14 + 3

        private decimal Overbought { get; }
        private decimal Oversold { get; }
        private decimal Threshold { get; }
        
        private decimal? StopLoss { get; }

        private State State { get; set; }
        
        private decimal LastHigh { get; set; }
        private decimal LastLow { get; set; }

        public List<Candle> Candles { get; set; }
        public string Ticker { get; }


        public StochLessMoreStrategy(string ticker, decimal overbought, decimal oversold, decimal threshold, decimal? stopLoss = null)
        {
            Ticker = ticker;

            Overbought = overbought;
            Oversold = oversold;
            Threshold = threshold;

            StopLoss = stopLoss / 100;
            
            LastHigh = 0;
            LastLow = 100;
        }

        public void Init(List<Candle> candles)
        {
            Candles = candles;
            State = new State {PositionState = PositionState.Sold, BuyPrice = -1, SellPrice = -1};
        }

        private bool CanProceed()
        {
            for (var i = 1; i < Candles.Count; i++)
            {
                var pred = Candles[i - 1];
                var current = Candles[i];

                if (current.Time - pred.Time != TimeSpan.FromMinutes(1))
                {
                    return false;
                }
            }

            return true;
        }

        public StrategyResult ProcessTradingStep(Candle candle)
        {
            if (Candles == null || Candles.Count < 17)
            {
                Logger.Error("It's too little base candles");
                throw new Exception("It's too little base candles.");
            }

            Candles.Add(candle);
            if (!CanProceed())
            {
                Logger.Error("Candles in inconsistent state");
                throw new Exception("Candles in inconsistent state");
            }

            var stoch = Candles.STOCH();

            StrategyResult result;
            switch (State.PositionState)
            {
                case PositionState.Sold when stoch - LastLow > Threshold:
                    State.Buy(candle.Close);
                    result = StrategyResult.Buy;
                    Logger.Info($"Bought {Ticker}, candle: {candle.Close} {candle.Time.OnlyTime()}, LastLow: {Math.Round(LastLow, 2)}, Stoch {Math.Round(stoch, 2)}");
                    break;
                case PositionState.Bought when LastHigh - stoch > Threshold:
                    State.Sell(candle.Close);
                    result = StrategyResult.Sell;
                    Logger.Info($"Sold {Ticker}, Candle: {candle.Close} {candle.Time.OnlyTime()}, Last high: {Math.Round(LastHigh, 2)}, Stoch: {Math.Round(stoch, 2)}");
                    break;
                case PositionState.Bought when StopLoss != null && (State.BuyPrice - candle.Close) / State.BuyPrice >= StopLoss:
                    State.Sell(candle.Close);
                    result = StrategyResult.Sell;
                    Logger.Info($"Sold by Stop Loss {Ticker}, Candle: {candle.Close} {candle.Time.OnlyTime()}, Last high: {Math.Round(LastHigh, 2)}, Stoch: {Math.Round(stoch, 2)}");
                    break;
                default:
                    result = StrategyResult.NoTrade;
                    Logger.Debug($"No Trade {Ticker}, Candle: {candle.Close} {candle.Time.OnlyTime()}, LastLow: {Math.Round(LastLow, 2)}, LastHigh: {Math.Round(LastHigh, 2)}, Stoch {Math.Round(stoch, 2)}");
                    break;
            }

            AddStochValue(stoch);
            return result;
        }

        public void Reset()
        {
            Candles = null;
            LastHigh = 0;
            LastLow = 100;
        }

        private void AddStochValue(decimal stoch)
        {
            LastHigh = stoch > LastHigh && stoch > Overbought ? stoch : stoch > Overbought ? LastHigh : 0;
            LastLow = stoch < LastLow && stoch < Oversold ? stoch : stoch < Oversold ? LastLow : 100;
        }
    }
}
