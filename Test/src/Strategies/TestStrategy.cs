using System;
using System.Collections.Generic;
using System.Linq;
using FinanceManagement.Bot.Strategies;
using FinanceManagement.Common;
using NUnit.Framework;

namespace Test.Strategies
{
    public enum TestStrategyEvent
    {
        Empty = 0,
        NotReady = 1,
        SoldByNotReady = 2,
        UnsatisfyingMarket = 3,
        SoldByUnsatisfyingMarket = 4,
        BoughtByStrategy = 5,
        SoldByStrategy = 6,
        SoldByStopLoss = 7,
        SoldByTakeProfit = 7,
        NoEvents = 8
    }
    
    public class TestStrategy: StrategyBase
    {
        private const decimal Info = 12345.6789m;

        public PositionState PositionState => State.PositionState;

        public TestStrategyEvent LastEvent => Events.LastOrDefault().Event;
        public List<(TestStrategyEvent Event, object[] Args, StrategyAction Result)> Events { get; set; } = new();
        public int OnProcessingCount { get; set; }
        public int OnProcessedCount { get; set; }

        private bool _shouldSell;
        private bool _shouldBuy;
        private bool _shouldProcess = true;
        
        public TestStrategy(decimal? takeProfit = null, decimal? stopLoss = null, TimeSpan? waitAfterStopLoss = null) : 
            base(new StrategyOptions
            {
                Instrument = "TEST",
                TakeProfit = takeProfit,
                StopLoss = stopLoss,
                WaitAfterStopLoss = waitAfterStopLoss
            })
        {
        }

        public override int BaseCandlesAmount => 3;

        public void ShouldBeProcessed(bool should) => _shouldProcess = should;
        public void ShouldBeSoldOnce() => _shouldSell = true;
        public void ShouldBeBoughtOnce() => _shouldBuy = true;

        protected override bool OnProcessing(out object info)
        {
            info = Info;
            OnProcessingCount++;
            
            return _shouldProcess;
        }
        
        protected override void OnProcessed(object info)
        {
            Assert.AreEqual(Info, info);
            OnProcessedCount++;
        }

        protected override bool ShouldSell(object info)
        {
            Assert.AreEqual(Info, info);

            if (_shouldSell)
            {
                _shouldSell = false;
                return true;
            }

            return false;
        }

        protected override bool ShouldBuy(object info)
        {
            Assert.AreEqual(Info, info);

            if (_shouldBuy)
            {
                _shouldBuy = false;
                return true;
            }

            return false;
        }

        protected override StrategyAction SellByNotReady()
        {
            var result = base.SellByNotReady();
            
            Events.Add((TestStrategyEvent.SoldByNotReady, Array.Empty<object>(), result));
            
            return result;
        }

        protected override StrategyAction NotReady()
        {
            var result = base.NotReady();
            
            Events.Add((TestStrategyEvent.NotReady, Array.Empty<object>(), result));
            
            return result;
        }

        protected override StrategyAction UnsatisfyingMarket(Candle candle)
        {
            var result = base.UnsatisfyingMarket(candle);
            
            Events.Add((TestStrategyEvent.UnsatisfyingMarket, new object[]{candle}, result));
            
            return result;
        }

        protected override StrategyAction SellByUnsatisfyingMarket(Candle candle)
        {
            var result = base.SellByUnsatisfyingMarket(candle);
            
            Events.Add((TestStrategyEvent.SoldByUnsatisfyingMarket, new object[]{candle}, result));
            
            return result;
        }

        protected override StrategyAction BuyByStrategy(Candle candle, object info)
        {
            var result = base.BuyByStrategy(candle, info);
            
            Events.Add((TestStrategyEvent.BoughtByStrategy, new[]{candle, info}, result));
            
            return result;
        }

        protected override StrategyAction SellByStrategy(Candle candle, object info)
        {
            var result = base.SellByStrategy(candle, info);
            
            Events.Add((TestStrategyEvent.SoldByStrategy, new[]{candle, info}, result));
            
            return result;
        }

        protected override StrategyAction SellByStopLoss(Candle candle, object info, TimeSpan skipTimeSpan = default)
        {
            var result = base.SellByStopLoss(candle, info, skipTimeSpan);
            
            Events.Add((TestStrategyEvent.SoldByStopLoss, new[]{candle, info, skipTimeSpan}, result));
            
            return result;
        }

        protected override StrategyAction SellByTakeProfit(Candle candle, object info)
        {
            var result = base.SellByTakeProfit(candle, info);
            
            Events.Add((TestStrategyEvent.SoldByTakeProfit, new[]{candle, info}, result));
            
            return result;
        }

        protected override StrategyAction NoEvents(Candle candle, object info)
        {
            var result = base.NoEvents(candle, info);
            
            Events.Add((TestStrategyEvent.NoEvents, new[]{candle, info}, result));
            
            return result;
        }
    }
}