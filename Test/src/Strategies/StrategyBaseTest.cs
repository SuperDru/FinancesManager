using System;
using System.Linq;
using FinanceManagement.Bot.Strategies;
using FinanceManagement.Common;
using NUnit.Framework;

namespace Test.Strategies
{
    public class StrategyBaseTest
    {
        [Test]
        public void NotReadyUntilCandlesEnough()
        {
            var strategy = new TestStrategy();

            for (var i = 0; i < strategy.BaseCandlesAmount; i++)
            {
                Assert.AreEqual(PositionState.NotReady, strategy.PositionState);
                NewCandle(strategy);
            }
            Assert.AreEqual(PositionState.Sold, strategy.PositionState);
        }
        
        [Test]
        public void NotReadyWhenInconsistentCandles()
        {
            var strategy = new TestStrategy();
            AddBaseCandles(strategy);
            
            var candles = strategy.State.Candles.Minute;

            NewCandle(strategy, candles.Last().Time.AddMinutes(2));
            for (var i = 0; i < strategy.BaseCandlesAmount - 1; i++)
            {
                Assert.AreEqual(PositionState.NotReady, strategy.PositionState);
                NewCandle(strategy);
            }
            Assert.AreEqual(PositionState.Sold, strategy.PositionState);
        }

        [Test]
        public void NotReadyWhenNotLatestCandle()
        {
            var strategy = new TestStrategy();
            AddBaseCandles(strategy);
            
            var candles = strategy.State.Candles.Minute;
            
            NewCandle(strategy, candles.Last().Time);
            Assert.AreEqual(PositionState.NotReady, strategy.PositionState);
            NewCandle(strategy);
            Assert.AreEqual(PositionState.Sold, strategy.PositionState);
            
            NewCandle(strategy, candles.Last().Time.AddMinutes(-2));
            Assert.AreEqual(PositionState.NotReady, strategy.PositionState);
            NewCandle(strategy);
            Assert.AreEqual(PositionState.Sold, strategy.PositionState);
        }
        
        [Test]
        public void SellIfBoughtAndNotReady()
        {
            var strategy = new TestStrategy();
            AddBaseCandles(strategy);
            
            strategy.ShouldBeBoughtOnce();
            NewCandle(strategy);
            Assert.AreEqual(TestStrategyEvent.BoughtByStrategy, strategy.LastEvent);
            
            var candles = strategy.State.Candles.Minute;
            NewCandle(strategy, candles.Last().Time.AddMinutes(2));
            Assert.AreEqual(TestStrategyEvent.SoldByNotReady, strategy.LastEvent);
        }
        
        [Test]
        public void DoesntBuyWhenNotReadyMarket()
        {
            var strategy = new TestStrategy();
            AddBaseCandles(strategy);
            
            var candles = strategy.State.Candles.Minute;
            
            strategy.ShouldBeBoughtOnce();
            Assert.AreEqual(StrategyAction.NoTrade, NewCandle(strategy, candles.Last().Time.AddMinutes(2)));
            Assert.AreEqual(TestStrategyEvent.NotReady, strategy.LastEvent);
        }
        
        [Test]
        public void UnsatisfyingMarket()
        {
            var strategy = new TestStrategy();
            AddBaseCandles(strategy);
            
            strategy.ShouldBeProcessed(false);
            Assert.AreEqual(StrategyAction.NoTrade, NewCandle(strategy));
            Assert.AreEqual(TestStrategyEvent.UnsatisfyingMarket, strategy.LastEvent);
            
            Assert.AreEqual(StrategyAction.NoTrade, NewCandle(strategy));
            Assert.AreEqual(TestStrategyEvent.UnsatisfyingMarket, strategy.LastEvent);
            
            strategy.ShouldBeProcessed(true);
            Assert.AreEqual(StrategyAction.NoTrade, NewCandle(strategy));
            Assert.AreEqual(TestStrategyEvent.NoEvents, strategy.LastEvent);
        }
        
        [Test]
        public void SellIfBoughtAndUnsatisfyingMarket()
        {
            var strategy = new TestStrategy();
            AddBaseCandles(strategy);
            
            strategy.ShouldBeBoughtOnce();
            Assert.AreEqual(StrategyAction.Buy, NewCandle(strategy));
            Assert.AreEqual(TestStrategyEvent.BoughtByStrategy, strategy.LastEvent);

            strategy.ShouldBeProcessed(false);
            Assert.AreEqual(StrategyAction.Sell, NewCandle(strategy));
            Assert.AreEqual(TestStrategyEvent.SoldByUnsatisfyingMarket, strategy.LastEvent);

            strategy.ShouldBeProcessed(true);
            Assert.AreEqual(StrategyAction.NoTrade, NewCandle(strategy));
            Assert.AreEqual(TestStrategyEvent.NoEvents, strategy.LastEvent);
        }
        
        [Test]
        public void DoesntBuyWhenUnsatisfyingMarket()
        {
            var strategy = new TestStrategy();
            AddBaseCandles(strategy);
            
            strategy.ShouldBeProcessed(false);
            strategy.ShouldBeBoughtOnce();
            Assert.AreEqual(StrategyAction.NoTrade, NewCandle(strategy));
            Assert.AreEqual(TestStrategyEvent.UnsatisfyingMarket, strategy.LastEvent);
        }
        
        [Test]
        public void BuyByStrategy()
        {
            var strategy = new TestStrategy();
            AddBaseCandles(strategy);
            
            strategy.ShouldBeBoughtOnce();
            Assert.AreEqual(StrategyAction.Buy, NewCandle(strategy));
            Assert.AreEqual(TestStrategyEvent.BoughtByStrategy, strategy.LastEvent);
        }
        
        [Test]
        public void SellByStrategy()
        {
            var strategy = new TestStrategy();
            AddBaseCandles(strategy);
            
            strategy.ShouldBeBoughtOnce();
            Assert.AreEqual(StrategyAction.Buy, NewCandle(strategy));
            
            strategy.ShouldBeSoldOnce();
            Assert.AreEqual(StrategyAction.Sell, NewCandle(strategy));
            Assert.AreEqual(TestStrategyEvent.SoldByStrategy, strategy.LastEvent);
        }
        
        [Test]
        public void DoesntSellIfNotBought()
        {
            var strategy = new TestStrategy();
            AddBaseCandles(strategy);
            
            strategy.ShouldBeSoldOnce();
            Assert.AreEqual(StrategyAction.NoTrade, NewCandle(strategy));
            Assert.AreEqual(TestStrategyEvent.NoEvents, strategy.LastEvent);
        }
        
        [Test]
        public void SellByStopLoss()
        {
            var strategy = new TestStrategy(stopLoss: 5, waitAfterStopLoss: TimeSpan.FromMinutes(3));
            AddBaseCandles(strategy);
            
            strategy.ShouldBeBoughtOnce();
            Assert.AreEqual(StrategyAction.Buy, NewCandle(strategy, close: 2600));
            Assert.AreEqual(StrategyAction.NoTrade, NewCandle(strategy));
            Assert.AreEqual(StrategyAction.Sell, NewCandle(strategy, close: 2450));
            Assert.AreEqual(TestStrategyEvent.SoldByStopLoss, strategy.LastEvent);
            
            strategy.ShouldBeBoughtOnce();
            Assert.AreEqual(StrategyAction.NoTrade, NewCandle(strategy));
            Assert.AreEqual(StrategyAction.NoTrade, NewCandle(strategy));
            Assert.AreEqual(StrategyAction.NoTrade, NewCandle(strategy));
            Assert.AreEqual(StrategyAction.Buy, NewCandle(strategy));
        }
        
        [Test]
        public void SellByTakeProfit()
        {
            var strategy = new TestStrategy(5);
            AddBaseCandles(strategy);
            
            strategy.ShouldBeBoughtOnce();
            Assert.AreEqual(StrategyAction.Buy, NewCandle(strategy, close: 2450));
            Assert.AreEqual(StrategyAction.NoTrade, NewCandle(strategy));
            Assert.AreEqual(StrategyAction.Sell, NewCandle(strategy, close: 2600));
            Assert.AreEqual(TestStrategyEvent.SoldByTakeProfit, strategy.LastEvent);
        }

        private static StrategyAction NewCandle(StrategyBase strategy,
                                decimal open = 2500m,
                                decimal close = 2550,
                                decimal high = 2600m,
                                decimal low = 2450m)
        {
            var candles = strategy.State.Candles.Minute;
            var time = candles.LastOrDefault()?.Time.AddMinutes(1) ??
                       new DateTime(2021, 1, 1, 1, 0, 0);
            return NewCandle(strategy, time, open, close, high, low);
        }
        
        private static StrategyAction NewCandle(IStrategy strategy,
                               DateTime time,
                               decimal open = 2500m,
                               decimal close = 2550,
                               decimal high = 2600m,
                               decimal low = 2450m)
        {
            return strategy.ProcessTradingStep(new Candle
            {
                Interval = Interval.Minute,
                Time = time,
                Open = open,
                Close = close,
                High = high,
                Low = low
            });
        }

        private static void AddBaseCandles(TestStrategy strategy)
        {
            for (var i = 0; i < strategy.BaseCandlesAmount; i++)
            {
                Assert.AreEqual(PositionState.NotReady, strategy.PositionState);
                NewCandle(strategy);
            }
            Assert.AreEqual(PositionState.Sold, strategy.PositionState);
        }
    }
}