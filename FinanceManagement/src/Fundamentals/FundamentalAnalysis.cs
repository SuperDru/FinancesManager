using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using FinanceManagement.Common;
using FinanceManagement.Yahoo;

namespace FinanceManagement.Fundamentals
{
    public static class FundamentalAnalysis
    {
        public static async Task<Fundamentals> LoadFundamentals(this YahooFinanceApi api, string instrument)
        {
            var financials = await api.LoadFinancials(instrument.ToUpper());
            
            var marginOfSafety = 0.15m;
            if (financials.GrowthRate > 1m)
            {
                marginOfSafety = 0.3m;
                Logger.Warning($"Surprisingly large growth rate for {financials.Ticker}: {financials.GrowthRate:P}");
            }
            var dcf = financials.DiscountedCashFlow(margin: marginOfSafety);
            var fundamentals = new Fundamentals
            {
                Instrument = financials.Ticker,
                DiscountedCashFlow = dcf,
                IntrinsicPrice = dcf / financials.OutstandingShares,
                MarketPrice = financials.MarketCapitalization / financials.OutstandingShares
            };

            Logger.Info($"Proceed fundamental analysis. {fundamentals}");

            return fundamentals;
        }

        public static async Task<MarketFinancials> LoadFinancials(this YahooFinanceApi api, string instrument)
        {
            var fileName = $"{Path.GetFullPath("../../../")}/FinanceData/{instrument}.xml";
            var ser = new DataContractSerializer(typeof(MarketFinancials));

            MarketFinancials financials;
            if (File.Exists(fileName))
            {
                await using var fs = new FileStream(fileName, FileMode.Open);
                using var reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                financials = (MarketFinancials) ser.ReadObject(reader);
                if (financials.Price >= 0) return financials;
            }
            
            financials = await api.GetMarketFinancials(instrument);
            await using var writer = new FileStream(fileName, FileMode.Create);
            ser.WriteObject(writer, financials);

            return financials;
        }
    }
}