using FinancesManager.Common;

namespace FinancesManager.Yahoo
{
    public class YahooOptions: IOptions
    {
        public string Name => "YahooApi";

        public string Url { get; set; }
        public string Key { get; set; }
        public string Host { get; set; }
    }
}