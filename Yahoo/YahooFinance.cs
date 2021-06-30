using System;
using System.Net.Http;
using System.Threading.Tasks;
using FinancesManager.Fundamentals;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FinancesManager.Common;

namespace FinancesManager.Yahoo
{
    public class YahooFinance
    {
        private readonly HttpClient _client;
        
        public YahooFinance()
        {
            var options = Configuration.Load<YahooOptions>();
            _client = new HttpClient
            {
                BaseAddress = new Uri(options.Url),
                DefaultRequestHeaders =
                {
                    {"x-rapidapi-key", options.Key},
                    {"x-rapidapi-host", options.Host}
                }
            };
        }

        public async Task<MarketFinancials> GetMarketFinancials(string ticker)
        {
            var financials = await GetFinancials(ticker);
            var analysis = await GetAnalysis(ticker);
            var statistics = await GetStatistics(ticker);
            
            var cashFlowStatement = financials["cashflowStatementHistory"]["cashflowStatements"][0];
            var operationsCashFlow = decimal.Parse(cashFlowStatement["totalCashFromOperatingActivities"]["raw"].ToString());
            var capitalExpenditures = decimal.Parse(cashFlowStatement["capitalExpenditures"]["raw"].ToString());
            var freeCashFlow = operationsCashFlow + capitalExpenditures; // positive + negative

            var growthRate = decimal.Parse(analysis["earningsTrend"]["trend"].Last.Previous["growth"]["raw"].ToString());
            
            var marketCapitalization = decimal.Parse(statistics["summaryDetail"]["marketCap"]["raw"].ToString());
            var enterpriseValue = decimal.Parse(statistics["defaultKeyStatistics"]["enterpriseValue"]["raw"].ToString());
            var outstandingShares = long.Parse(statistics["defaultKeyStatistics"]["sharesOutstanding"]["raw"].ToString());
            var marketPrice = marketCapitalization / outstandingShares;
            
            var result = new MarketFinancials
            {
                Ticker = ticker,
                FreeCashFlow = freeCashFlow,
                GrowthRate = growthRate,
                MarketCapitalization = marketCapitalization,
                EnterpriseValue = enterpriseValue,
                OutstandingShares = outstandingShares,
                Price = marketPrice
            };

            Logger.Info($"Loaded from yahoo finance. {result}");
            
            return result;
        }
        
        private async Task<JObject> GetFinancials(string ticker)
        {
            var res = await _client.GetAsync($"/stock/v2/get-financials?symbol={ticker}");
            var str = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<JObject>(str);
        } 
        
        private async Task<JObject> GetAnalysis(string ticker)
        {
            var res = await _client.GetAsync($"/stock/v2/get-analysis?symbol={ticker}");
            var str = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<JObject>(str);
        }
        
        private async Task<JObject> GetStatistics(string ticker)
        {
            var res = await _client.GetAsync($"/stock/v3/get-statistics?symbol={ticker}");
            var str = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<JObject>(str);
        } 
    }
}