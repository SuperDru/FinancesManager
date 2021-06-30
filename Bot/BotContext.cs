using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Timers;
using FinancesManager.Common;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;

namespace FinancesManager.Bot
{
    public enum LaunchType
    {
        RealTime = 1,
        BackTest = 2
    }
    
    public class BotContext
    {
        private Connection Connection { get; }
        private MarketInstrument Market { get; }
        private IStrategy Strategy { get; }
        private StrategyStatistics Statistics { get; }
        
        private List<CandlePayload> Candles { get; set; }
        private SortedDictionary<DateTime, CandlePayload> ProcessingCandles { get; }

        public BotContext(Connection conn, MarketInstrument market, IStrategy strategy)
        {
            Connection = conn;
            Market = market;
            Strategy = strategy;

            Statistics = new StrategyStatistics(Market.Ticker, Market.Figi);

            ProcessingCandles = new SortedDictionary<DateTime, CandlePayload>();
        }

        public async Task Start()
        {
            Connection.StreamingEventReceived += OnEvent;
            Connection.WebSocketException += OnException;
            Connection.StreamingClosed += OnStreamingClosed;
            await Connection.SendStreamingRequestAsync(StreamingRequest.SubscribeCandle(Market.Figi, CandleInterval.Minute));
            await InitStrategy();
        }
        
        public async Task StartBackTest(DateTime from, DateTime to)
        {
            for (var current = from; current < to; current = current.AddDays(1))
            {
                // var startDate = new DateTime(current.Year, current.Month, current.Day, 17, 30, 0);
                // var endDate = new DateTime(current.Year, current.Month, current.Day, 23, 59, 0);
                
                var startDate = new DateTime(current.Year, current.Month, current.Day, 10, 30, 0);
                var endDate = new DateTime(current.Year, current.Month, current.Day, 17, 30, 0);

                var candlesList = await Connection.Context.MarketCandlesAsync(Market.Figi, startDate, endDate, CandleInterval.Minute);
                var candles = candlesList.Candles;
                
                if (Candles == null)
                {
                    Candles = candlesList.Candles.Take(Strategy.BaseCandlesAmount).ToList();
                    candles = candles.Skip(Strategy.BaseCandlesAmount).ToList();

                    if (Candles.Count < Strategy.BaseCandlesAmount)
                    {
                        Candles = null;
                        continue;
                    }
                    
                    Strategy.Init(Candles);
                }

                var lastCandle = Candles.Last();
                foreach (var candle in candles)
                {
                    while (candle.Time - lastCandle.Time != TimeSpan.FromMinutes(1))
                    {
                        lastCandle = lastCandle.EmptyCandle();
                        Trade(Strategy.ProcessTradingStep(lastCandle), lastCandle);
                    }
                    Trade(Strategy.ProcessTradingStep(candle), candle);
                    lastCandle = candle;
                }
                // Statistics.Short(candles.Last().Open, Statistics.Positions.Sum(x => x.Value));
                // Logger.Info($"Close position. Statistics: {Statistics}");
                Statistics.Positions.Clear();
                Strategy.Reset();
                Candles = null;
            }
        }

        private async Task InitStrategy()
        {
            var from = DateTime.Now.AddMinutes(-Strategy.BaseCandlesAmount - 100);
            var to = DateTime.Now.AddMinutes(1);
            var candlesResponse = await Connection.Context.MarketCandlesAsync(Market.Figi, from, to, CandleInterval.Minute);
            var baseCandles = candlesResponse.Candles.TakeLast(Strategy.BaseCandlesAmount);
            Candles = baseCandles.ToList();
            Strategy.Init(Candles);

            var timer = new Timer(1000);
            timer.Elapsed += StepOnNewMinute;
            timer.Start();
        }

        private void OnEvent(object sender, StreamingEventReceivedEventArgs e)
        {
            if (e.Response is CandleResponse candle)
            {
                Logger.Debug($"New candle {Market.Ticker} {candle.Payload.Time.OnlyTime()} {candle.Payload.Close}");
                ProcessingCandles[candle.Payload.Time] = candle.Payload;
                return;
            }
            Logger.Debug($"OnEvent Without Handler, {Market.Ticker}, {e.Response.Event}");
        }
        
        private void OnException(object sender, WebSocketException e)
        {
            Logger.Error($"Web socket exception, {Market.Ticker}", e);
        }
        
        private void OnStreamingClosed(object sender, EventArgs e)
        {
            Logger.Info($"Web socket streaming closed, {Market.Ticker}");
        }
        
        private void StepOnNewMinute(object sender, ElapsedEventArgs e)
        {
            var now = DateTime.Now;
            // Logger.Info($"Tick, {Market.Ticker}, {now}");
            var last = now.AddMinutes(-1);
            if (now.Second != 1) return;

            var currentLast = Candles.Last();
            if (currentLast.Time.Minute == last.Minute) return;

            var candle = ProcessingCandles.FirstOrDefault(___ => ___.Key.Minute == last.Minute).Value;
            if (candle == null)
            {
                candle = currentLast.EmptyCandle();
                Logger.Warning($"Added empty candle, {Market.Ticker}, {candle.Time}");
            }
            else
            {
                Logger.Debug($"Candle for next step, {Market.Ticker}, {candle.Close} {candle.Time}");
                ProcessingCandles.Remove(ProcessingCandles.First().Key);
            }
            Candles.Add(candle);
            Trade(Strategy.ProcessTradingStep(candle), candle);
        }
        
        private void Trade(StrategyResult strategyResult, CandlePayload candle)
        {
            switch (strategyResult)
            {
                case StrategyResult.Buy: //when candle.Time.Hour < 20:
                    Statistics.Long(candle.Close, 1);
                    break;
                case StrategyResult.Sell:// when Statistics.Positions.Count > 0:
                    Statistics.Short(candle.Close, 1);
                    Logger.Info($"Close position. Statistics: {Statistics}");
                    break;
                case StrategyResult.NoTrade:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(strategyResult), strategyResult, null);
            }
        }

        private List<CandlePayload> MakeConsistent(List<CandlePayload> candles)
        {
            return candles;
        }
    }
}