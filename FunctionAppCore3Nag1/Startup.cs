using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Services.Services;


[assembly: FunctionsStartup(typeof(FunctionAppCore3Nag1.Startup))]

namespace FunctionAppCore3Nag1
{
    public class Startup : FunctionsStartup
    {

        // This Startup class is used declare various implementations so that they could be injected at run  time 
        // into the function

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IStockQuoteService, StockQuoteService>();

            /*
            builder.Services.AddHttpClient();
             
            builder.Services.AddSingleton((s) => {
                return new MyService();
            });
            
            builder.Services.AddSingleton<ILoggerProvider, MyLoggerProvider>();
            */
        }
    }
}