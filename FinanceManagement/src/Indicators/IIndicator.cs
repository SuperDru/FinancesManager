using System;
using System.Collections.Generic;
using FinanceManagement.Common;

namespace FinanceManagement.Indicators
{
    public interface IIndicator
    {
        List<object> Series { get; }
    }

    public interface ICandleIndicator : IIndicator
    {
        void Push(Candle candle, bool changed = false);
    }
    
    public interface IDecimalIndicator : IIndicator
    {
        void Push(decimal candle, bool changed = false);
    }
}
