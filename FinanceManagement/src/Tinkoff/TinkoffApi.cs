using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinanceManagement.Bot;
using FinanceManagement.Common;
using Microsoft.Extensions.Options;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;

namespace FinanceManagement.Tinkoff
{
    public class TinkoffApi
    {
        public Context Context { get; }
        
        public TinkoffApi(IOptions<TinkoffOptions> tinkoffOptions)
        {
            var options = tinkoffOptions?.Value ?? throw new ArgumentException("Options cannot be null", nameof(tinkoffOptions));
            var conn = ConnectionFactory.GetConnection(options.Token);
            Context = conn.Context;
        }

        public async Task<List<MarketInstrument>> GetInstrumentsAsync(Func<MarketInstrument, bool> filter = default)
        {
            var res = await Context.MarketStocksAsync();

            var instruments = res.Instruments as IEnumerable<MarketInstrument>;
            if (filter != null)
            {
                instruments = instruments.Where(filter);
            }
            
            return instruments.ToList();
        }
    }
}