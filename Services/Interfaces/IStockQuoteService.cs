using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServiceReference1;


namespace Services.Interfaces
{
    public interface IStockQuoteService
    {
        Task<QuoteData> GetQuoteAsync(string StockSymbol, string LicenseKey);

        Task<decimal> GetQuickQuoteAsync(string StockSymbol, string LicenseKey);

        Task<ArrayOfXElement> GetQuoteDataSetAsync(string StockSymbols, string LicenseKey);
    }
}