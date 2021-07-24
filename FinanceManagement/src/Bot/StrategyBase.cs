using System;
using System.Collections.Generic;
using System.Linq;
using FinanceManagement.Common;

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

    public class CandlesStorage
    {
        public int Count => MinuteCandles.Count;
        public List<Candle> MinuteCandles { get; set; }
        public List<Candle> HourCandles { get; set; }

        public void AddMinuteCandle(Candle candle)
        {
            if (candle.Interval != Interval.Minute) throw new Exception("Expected a candle with minute interval");
            MinuteCandles.Add(candle);
            var lastHourCandle = HourCandles.LastOrDefault();
            if (lastHourCandle?.Time.Hour != candle.Time.Hour)
            {
                var newHourCandle = new Candle
                {
                    Interval = Interval.Hour,
                    Time = candle.Time.AddMinutes(-candle.Time.Minute),
                    Open = candle.Open,
                    Close = candle.Close,
                    High = candle.High,
                    Low = candle.Low,
                    Volume = candle.Volume
                };
                HourCandles.Add(newHourCandle);
                return;
            }

            lastHourCandle.Close = candle.Close;
            lastHourCandle.Low = Math.Min(lastHourCandle.Low, candle.Low);
            lastHourCandle.High = Math.Max(lastHourCandle.High, candle.High);
            lastHourCandle.Volume += candle.Volume;
        }
    }
    
    public abstract class StrategyBase: IStrategy
    {
        public abstract int BaseCandlesAmount { get; }

        protected CandlesStorage CandlesStorage { get; private set; }

        private string Instrument { get; }
        private State State { get; set; }

        private decimal? TakeProfit { get; }
        private decimal? StopLoss { get; }

        protected StrategyBase(string instrument,  decimal? takeProfit, decimal? stopLoss)
        {
            Instrument = instrument;
            TakeProfit = takeProfit / 100m;
            StopLoss = stopLoss / 100m;
        }

        public virtual void Reset()
        {
            CandlesStorage = null;
        }
        
        public virtual void Init(List<Candle> candles)
        {
            CandlesStorage = new CandlesStorage();
            foreach (var candle in candles)
            {
                CandlesStorage.AddMinuteCandle(candle);
            }
            State = new State {PositionState = PositionState.Sold, BuyPrice = -1, SellPrice = -1};
        }

        public StrategyAction ProcessTradingStep(Candle candle)
        {
            if (CandlesStorage == null || CandlesStorage.Count < BaseCandlesAmount)
            {
                Logger.Error("It's too little base candles");
                throw new Exception("It's too little base candles.");
            }

            AddCandle(candle);

            var info = CalculateSpecificInfo();

            StrategyAction action;
            switch (State.PositionState)
            {
                case PositionState.Sold when ShouldBuy(info):
                    State.Buy(candle.Close);
                    action = StrategyAction.Buy;
                    Logger.Info($"Bought {Instrument}, candle: {candle.Close} {candle.Time.OnlyTime()}");
                    break;
                case PositionState.Bought when ShouldSell(info):
                    State.Sell(candle.Close);
                    action = StrategyAction.Sell;
                    Logger.Info($"Sold {Instrument}, Candle: {candle.Close} {candle.Time.OnlyTime()}");
                    break;
                case PositionState.Bought when StopLoss != null && (State.BuyPrice - candle.Close) / State.BuyPrice >= StopLoss:
                    State.Sell(candle.Close);
                    action = StrategyAction.Sell;
                    Logger.Info($"Sold by Stop Loss {Instrument}, Candle: {candle.Close} {candle.Time.OnlyTime()}");
                    break;
                case PositionState.Bought when TakeProfit != null && (candle.Close - State.BuyPrice) / State.BuyPrice >= TakeProfit:
                    State.Sell(candle.Close);
                    action = StrategyAction.Sell;
                    Logger.Info($"Sold by Take Profit {Instrument}, Candle: {candle.Close} {candle.Time.OnlyTime()}");
                    break;
                default:
                    action = StrategyAction.NoTrade;
                    Logger.Debug($"No Trade {Instrument}, Candle: {candle.Close} {candle.Time.OnlyTime()}");
                    break;
            }

            OnProcessed(info);
            
            return action;
        }

        protected abstract bool ShouldSell(object info);
        protected abstract bool ShouldBuy(object info);
        protected abstract object CalculateSpecificInfo();
        protected abstract void OnProcessed(object info);
        
        private void AddCandle(Candle candle)
        {
            CandlesStorage.AddMinuteCandle(candle);
            
            var pred = CandlesStorage.MinuteCandles[^2];
            var current = CandlesStorage.MinuteCandles[^1];

            if (current.Time - pred.Time == TimeSpan.FromMinutes(1)) return;
            
            Logger.Error("Candles in inconsistent state");
            throw new Exception("Candles in inconsistent state");
        }
    }
}