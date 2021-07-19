using Tinkoff.Trading.OpenApi.Models;

namespace FinanceManagement.Income
{
    public class Income
    {
        public Currency Currency { get; set; }
        public string Figi { get; set; }
        public string Ticker { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
    }
}
