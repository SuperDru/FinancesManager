using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinanceManagement.Binance;
using FinanceManagement.Bot;
using FinanceManagement.Fundamentals;
using FinanceManagement.Tinkoff;
using FinanceManagement.Yahoo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FinanceManagement
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            AddFinanceApiServices(services, _configuration.GetSection("FinanceApi"));

            services.AddMvc();
            services.AddHostedService<BotHostedService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(_ => _.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static void AddFinanceApiServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<YahooFinanceApi>();
            services.Configure<YahooOptions>(configuration.GetSection(YahooOptions.Name));
            
            services.AddSingleton<TinkoffApi>();
            services.Configure<TinkoffOptions>(configuration.GetSection(TinkoffOptions.Name));
            
            services.AddSingleton<BinanceApi>();
            services.Configure<BinanceOptions>(configuration.GetSection(BinanceOptions.Name));
        }
    }
}
