using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinancesManager.Bot;
using FinancesManager.Common;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;

namespace FinancesManager.Tinkoff
{
    public class TinkoffApi
    {
        public Context Context { get; }
        
        public TinkoffApi()
        {
            var options = Configuration.Load<TinkoffOptions>();
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