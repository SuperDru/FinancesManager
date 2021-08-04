using System;
using FinanceManagement.Indicators;

namespace FinanceManagement.Common
{
    public class Candle
    {
        public Interval Interval { get; set; }
        
        public DateTime Time { get; set; }

        public decimal Open { get; set; }
        public decimal Close { get; set; }
        public decimal Low { get; set; }
        public decimal High { get; set; }

        public decimal Volume { get; set; }
    }
}