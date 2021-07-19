using FinanceManagement.Common;

namespace FinanceManagement.Yahoo
{
    public class YahooOptions
    {
        public static string Name => "Yahoo";

        public string Url { get; set; }
        public string Key { get; set; }
        public string Host { get; set; }
    }
}