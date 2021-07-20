using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinanceManagement.Common
{
    public interface IFinanceDataApi
    {
        Task<List<Candle>> GetCandlesAsync(string instrument, Interval interval = Interval.Minute, DateTime? from = null, DateTime? to = null);
    }
}