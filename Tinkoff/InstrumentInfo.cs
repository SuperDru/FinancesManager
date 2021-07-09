using Tinkoff.Trading.OpenApi.Models;

namespace FinancesManager.Tinkoff
{
    public class InstrumentInfo
    {
        public MarketInstrument BasicInfo { get; set; }
        public decimal? Price { get; set; }

        public InstrumentInfo(MarketInstrument baseInfo, decimal? price = null)
        {
            BasicInfo = baseInfo;
            Price = price;
        }

        public override string ToString()
        {
            return $"{BasicInfo.Ticker} - {BasicInfo.Name}" + (Price.HasValue ? $"({Price} {BasicInfo.Currency})" : "No recent price");
        }
    }
}