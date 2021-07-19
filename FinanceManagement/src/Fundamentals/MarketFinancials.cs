using System.Runtime.Serialization;
using FinanceManagement.Common;

namespace FinanceManagement.Fundamentals
{
    [DataContract]
    public class MarketFinancials
    {
        [DataMember]
        public string Ticker { get; set; }
        [DataMember]
        public decimal FreeCashFlow { get; set; }
        [DataMember]
        public decimal GrowthRate { get; set; }
        [DataMember]
        public decimal MarketCapitalization { get; set; }
        [DataMember]
        public decimal EnterpriseValue { get; set; }
        [DataMember]
        public decimal Price { get; set; }
        [DataMember]
        public long OutstandingShares { get; set; }
        
        public override string ToString() => $"Ticker {Ticker}: " +
                                             $"Free Cash Flow {FreeCashFlow.Billions():F2}; " +
                                             $"Growth Rate {GrowthRate:P}; " +
                                             $"Enterprise Value {EnterpriseValue.Billions():F2}; " +
                                             $"Price {Price:F2}";
    }
}