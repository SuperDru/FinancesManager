using System;
using System.Collections.Generic;
using System.Text;
using Tinkoff.Trading.OpenApi.Models;

namespace FinanceManagement.Indicators
{
    public interface IIndicator
    {
        decimal? Value => null;
        void Push(decimal value);
    }
}
