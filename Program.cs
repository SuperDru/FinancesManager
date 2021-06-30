using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using FinancesManager.Bot;
using FinancesManager.Common;
using Newtonsoft.Json.Linq;
using Tinkoff.Trading.OpenApi.Network;
using Tinkoff.Trading.OpenApi.Models;
using FinancesManager.Fundamentals;
using FinancesManager.Yahoo;

namespace FinancesManager
{
    public class Program
    {
        private static readonly DateTime StartDate = DateTime.Parse("10.09.2020");

        public static async Task Main()
        {
            var analysis = new FundamentalAnalysis("pins");
            await analysis.LoadFundamentals();

            #region Bots

            // var bot = new TradingBot(Token, new[] { "KO" }, 200);
            // try
            // {
            //     await bot.Start();
            //     while (true)
            //     {
            //         // Logger.Debug("TEST");
            //         await Task.Delay(TimeSpan.FromSeconds(100));
            //         // Task.Delay(TimeSpan.FromMinutes(1500)).GetAwaiter().GetResult();
            //     }
            // }
            // finally
            // {
            //     bot.Stop().GetAwaiter().GetResult();
            // }

            #endregion

            #region Income

            // var conn = ConnectionFactory.GetConnection(Token);
            // var context = conn.Context;
            //
            // var rubMarketsContext = new IncomeContext(context, Currency.Rub);
            // rubMarketsContext.Calculate(StartDate).GetAwaiter().GetResult();
            // Console.WriteLine(rubMarketsContext);
            //
            // var usdMarketsContext = new IncomeContext(context, Currency.Usd);
            // usdMarketsContext.Calculate(StartDate).GetAwaiter().GetResult();
            // Console.WriteLine(usdMarketsContext);
            //
            // Console.WriteLine($"Total income: {context.GetTotalClosedPositionsIncome(rubMarketsContext, usdMarketsContext).GetAwaiter().GetResult()};");

            #endregion
        }
    }
}
