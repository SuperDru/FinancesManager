using System.Collections.Generic;

namespace FinancesManager.Fundamentals
{
    public class Portfolio
    {
        public Dictionary<string, int> Shares { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> Bonds { get; set; } = new Dictionary<string, int>();

        public decimal SharesRatio { get; set; }

        public Portfolio(decimal sharesRatio = 0.8m)
        {
            SharesRatio = sharesRatio;
        }
    }
}