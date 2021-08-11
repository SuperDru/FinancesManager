// using System.Threading.Tasks;
// using FinanceManagement.Binance;
// using FinanceManagement.Bot.Backtesting;
// using FinanceManagement.Bot.Strategies;
// using FinanceManagement.Common;
// using Microsoft.AspNetCore.Mvc;
//
// namespace FinanceManagement.Controllers
// {
//     [Route("api/v1/compose")]
//     public class ComposeController: ControllerBase
//     {
//         private readonly BinanceApi _api;
//
//         public ComposeController(BinanceApi api)
//         {
//             _api = api;
//         }
//         
//         [HttpGet]
//         public async Task<ComposeResponse> Compose()
//         {
//             var candles = await _api.GetCandlesFromCsvAsync("ETHBUSD-1m-2021-05.csv");
//             var strategy = new StochLessMoreStrategy("ETHBUSD", 80m, 20m, 5m, 10m, 5m);
//             var context = new Backtest("ETHBUSD", strategy);
//             var backtest = context.Process(candles);
//             
//             var store = new CandlesStore();
//             foreach (var candle in candles)
//             {
//                 store.AddCandle(candle);
//             }
//
//             var response = new ComposeResponse
//             {
//                 Backtest = backtest,
//                 Candles = store.Candles[Interval.Hour],
//                 Indicators = ne
//             };
//         }
//     }
// }