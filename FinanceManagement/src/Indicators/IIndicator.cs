using System.Collections.Generic;
using FinanceManagement.Common;

namespace FinanceManagement.Indicators
{
    public interface IIndicator
    {
        List<object> Series { get; }
        void Push(Candle candle);
    }
}
