using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ServiceModel;
using ServiceReference1;
using System.Xml;
using Services.Interfaces;

namespace Services.Services
{
    public class StockQuoteService : IStockQuoteService
    {
        private static BasicHttpBinding binding;
        private static EndpointAddress address = new EndpointAddress("http://ws.cdyne.com/delayedstockquote/delayedstockquote.asmx");
        private static DelayedStockQuoteSoapClient client;

        public static int count = 0;

        public StockQuoteService()
        {
            binding = new BasicHttpBinding
            {
                SendTimeout = TimeSpan.FromSeconds(10000),
                MaxBufferSize = int.MaxValue,
                MaxReceivedMessageSize = int.MaxValue,
                AllowCookies = true,
                ReaderQuotas = XmlDictionaryReaderQuotas.Max
            };

            binding.Security.Mode = BasicHttpSecurityMode.None;

            client = new DelayedStockQuoteSoapClient(binding, address);
        }

        public Task<QuoteData> GetQuoteAsync(string stockSymbol, string licenseKey)
        {
            count++;
            return client.GetQuoteAsync(stockSymbol, licenseKey);
        }

        public Task<decimal> GetQuickQuoteAsync(string stockSymbol, string licenseKey)
        {
            return client.GetQuickQuoteAsync(stockSymbol, licenseKey);
        }

        public Task<ArrayOfXElement> GetQuoteDataSetAsync(string stockSymbol, string licenseKey)
        {
            return client.GetQuoteDataSetAsync(stockSymbol, licenseKey);
        }
    }
}
