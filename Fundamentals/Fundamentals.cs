using FinancesManager.Common;

namespace FinancesManager.Fundamentals
{
    public class Fundamentals
    {
        public string Instrument { get; set; }
        public decimal DiscountedCashFlow { get; set; }
        public decimal IntrinsicPrice { get; set; }
        public decimal MarketPrice { get; set; }

        public override string ToString() => $"Ticker {Instrument}: " +
                                             $"DCF {DiscountedCashFlow.Billions():F2}; " +
                                             $"Intrinsic Price {IntrinsicPrice:F4}; " +
                                             $"Market Price {MarketPrice:F4}";
    }
}