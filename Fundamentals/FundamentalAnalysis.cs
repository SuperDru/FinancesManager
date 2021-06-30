using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using FinancesManager.Common;
using FinancesManager.Yahoo;

namespace FinancesManager.Fundamentals
{
    public class FundamentalAnalysis
    {
        private readonly YahooFinance _yahooApi = new YahooFinance();

        public string Instrument { get; set; }

        public FundamentalAnalysis(string instrument)
        {
            Instrument = instrument.ToUpper();
        }

        public async Task<Fundamentals> LoadFundamentals()
        {
            var financials = await LoadFinancials();
            
            var marginOfSafety = 0.15m;
            if (financials.GrowthRate > 1m)
            {
                marginOfSafety = 0.3m;
                Logger.Warning($"Surprisingly large growth rate for {financials.Ticker}: {financials.GrowthRate:P}");
            }
            var dcf = financials.DiscountedCashFlow(margin: marginOfSafety);
            var fundamentals = new Fundamentals
            {
                Instrument = Instrument,
                DiscountedCashFlow = dcf,
                IntrinsicPrice = dcf / financials.OutstandingShares,
                MarketPrice = financials.MarketCapitalization / financials.OutstandingShares
            };

            Logger.Info($"Proceed fundamental analysis. {fundamentals}");

            return fundamentals;
        }

        private async Task<MarketFinancials> LoadFinancials()
        {
            var fileName = $"{Path.GetFullPath("../../../")}/FinanceData/{Instrument}.xml";
            var ser = new DataContractSerializer(typeof(MarketFinancials));

            MarketFinancials financials;
            if (File.Exists(fileName))
            {
                await using var fs = new FileStream(fileName, FileMode.Open);
                using var reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                financials = (MarketFinancials) ser.ReadObject(reader);
                if (financials.Price >= 0) return financials;
            }
            
            financials = await _yahooApi.GetMarketFinancials(Instrument);
            await using var writer = new FileStream(fileName, FileMode.Create);
            ser.WriteObject(writer, financials);

            return financials;
        }
    }
}