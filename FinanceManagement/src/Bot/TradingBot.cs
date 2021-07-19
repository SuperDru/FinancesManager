using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinanceManagement.Bot;
using FinanceManagement.Common;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;

namespace FinanceManagement.Bot
{
    public class TradingBot: IDisposable
    {
        private TinkoffOptions Options { get; }
        private int Balance { get; set; }
        private Currency Currency { get; }


        private Dictionary<string, MarketInstrument> Markets { get; set; }
        private Dictionary<string, StochLessMoreStrategy> Strategies { get; set; }
        private Dictionary<string, StrategyStatistics> Statistics { get; set; }
        private string[] AllTickers { get; }

        private readonly Dictionary<string, CandlePayload> _processingCandles;
        
        private List<BotContext> Bots { get; }

        public TradingBot(TinkoffOptions options, string token, string[] tickers, int balance = 100, Currency currency = Currency.Usd)
        {
            Balance = balance;
            Currency = currency;

            AllTickers = tickers;
            Options = options;
            
            Bots = new List<BotContext>();
            
            _processingCandles = new Dictionary<string, CandlePayload>();
            Statistics = new Dictionary<string, StrategyStatistics>();
        }

        public async Task Start()
        {
            Logger.Info("Bot start");

            Markets = (await FilterTickers(AllTickers)).ToDictionary(_ => _.Figi, _ => _);

            Logger.Info("Bots init");

            foreach (var (_, market) in Markets)
            {
                var conn = ConnectionFactory.GetConnection(Options.Token);
                var strategy = new StochLessMoreStrategy(market.Ticker, 91, 20, 5m);
                var bot = new BotContext(conn, market, strategy);
                // _ = bot.Start();
                _ = bot.StartBackTest(DateTime.Now.AddDays(-20), DateTime.Now.AddDays(-1));
                
                Bots.Add(bot);
            }
            
            // Strategies = new Dictionary<string, StochLessMoreStrategy>();
            // foreach (var instrument in Instruments.Values)
            // {
            //     var strategy = new StochLessMoreStrategy(Context, instrument.Figi, instrument.Ticker, 85, 20, 1.5m);
            //     await strategy.Init();
            //
            //     await Connection.SendStreamingRequestAsync(StreamingRequest.SubscribeCandle(instrument.Figi, CandleInterval.Minute));
            //     Strategies[instrument.Figi] = strategy;
            //     Statistics[instrument.Figi] = new StrategyStatistics(instrument.Ticker, instrument.Figi);
            // }

            // var timer = new System.Threading.Timer(_ =>
            // {
            //     var now = DateTime.Now;
            //
            // }, null, 0, 1000);


            Logger.Info("Bots init completed");

            // Connection.StreamingEventReceived += OnCandle;
        }

        private void OnCandle(object sender, StreamingEventReceivedEventArgs e)
        {
            if (!(e.Response is CandleResponse candle) || Strategies == null)
            {
                Logger.Debug($"No candle: {e.Response.Event}");
                return;
            }

            var figi = candle.Payload.Figi;
            var ticker = Markets[figi].Ticker;
            var strategy = Strategies[figi];

           Logger.Debug($"New candle {ticker} {candle.Time.OnlyTime()}");

            if (!_processingCandles.ContainsKey(figi) || _processingCandles[figi].Time == candle.Payload.Time)
            {
                _processingCandles[figi] = candle.Payload;
                return;
            }

            // var result = strategy.ProcessTradingStep(_processingCandles[figi]).GetAwaiter().GetResult();
            // Trade(result, figi).GetAwaiter().GetResult();

            _processingCandles[figi] = candle.Payload;
        }

        public async Task Stop()
        {
            Logger.Info("Bot stopping");

            // foreach (var instrument in Markets)
            // {
            //     await Connection.SendStreamingRequestAsync(StreamingRequest.UnsubscribeCandle(instrument.Key, CandleInterval.Minute));
            // }

            Logger.Info("Bot stopped");
        }


        private Task<MarketInstrument[]> FilterTickers(IEnumerable<string> tickers) =>
            Task.WhenAll(tickers.Select(async _ =>
            {
                var context = ConnectionFactory.GetConnection(Options.Token).Context;
                var marketResponse = await context.MarketSearchByTickerAsync(_);
                return marketResponse.Instruments.First();
            }));

        private async Task Trade(StrategyResult strategyResult, string figi)
        {
            if (!Markets.TryGetValue(figi, out var market))
            {
                return;
            }

            var candle =_processingCandles[figi];
            var statistics = Statistics[figi];

            switch (strategyResult)
            {
                case StrategyResult.Buy:
                    statistics.Long(candle.Close, 1);
                    break;
                case StrategyResult.Sell:
                    statistics.Short(candle.Close, 1);
                    Logger.Info($"Close position. Statistics: {statistics}");
                    break;
                case StrategyResult.NoTrade:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(strategyResult), strategyResult, null);
            }
        }

        public void Dispose()
        {
            Stop().GetAwaiter().GetResult();
        }
    }
}
