using FinancesManager.Common;

namespace FinancesManager.Bot
{
    public class TinkoffOptions: IOptions
    {
        public string Name => "TinkoffApi";

        public string Token { get; set; }
    }
}