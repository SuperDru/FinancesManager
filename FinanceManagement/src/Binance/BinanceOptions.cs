using FinanceManagement.Common;

namespace FinanceManagement.Binance
{
    public class BinanceOptions
    {
        public static string Name => "Binance";

        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
    }
}