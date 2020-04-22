using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.ServiceModel;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using ServiceReference1;
using FunctionAppCore3Nag1.Models;
using FunctionAppCore3Nag1.Utility;
using Services.Interfaces;
using Polly;


namespace FunctionAppCore3Nag1
{
    public class StockQuoteFunction
    {

        private IStockQuoteService _stockQuoteService;


        //https://gist.github.com/Kevin-Bronsdijk/dde6df58f429764a60f7815dd0052adf
        //https://www.hanselman.com/blog/AddingResilienceAndTransientFaultHandlingToYourNETCoreHttpClientWithPolly.aspx
        //https://github.com/TroyWitthoeft/AzureFunctionHttpClientFactoryPollyLogging
        // https://github.com/App-vNext/Polly
        // from jeff
        // https://dev.to/azure/serverless-circuit-breakers-with-durable-entities-3l2f
        // https://github.com/TroyWitthoeft/AzureFunctionHttpClientFactoryPollyLogging


        Policy policy = Policy.Handle<Exception>().WaitAndRetry(3,
                attempt => TimeSpan.FromSeconds(0.1 * Math.Pow(2, attempt)),
                (exception, calculatedWaitDuration) =>
                {
                    //log.LogInformation($"exception: {exception.Message}");
                });

        // backend service implementation gets injected through dependency injection
        public StockQuoteFunction(IStockQuoteService stockQuoteServiceClient)
        {
            _stockQuoteService = stockQuoteServiceClient;
        }


        [FunctionName("GetQuote")]
        //public static async Task<IActionResult> Run(
        //public async Task<String> GetQuote(
        public async Task<IActionResult> GetQuote(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            
            log.LogInformation("C# HTTP trigger function processed a request.");
            
            Stock stockSymbol = null;
            QuoteData response = null;


            try
            {
                stockSymbol = XmlUtility.GetStockDetails(req);
            } catch (Exception e)
            {

                /*
                The 422(Unprocessable Entity) status code means the server understands the content type of the request 
                entity(hence a 415(Unsupported Media Type) status code is inappropriate), and the syntax of the request 
                entity is correct(thus a 400(Bad Request) status code is inappropriate) but was unable to process the 
                contained instructions. For example, this error condition may occur if an XML request body contains 
                well - formed(i.e., syntactically correct), but semantically erroneous, XML instructions.

                BadRequestObjectResult resp = new BadRequestObjectResult("Invalid Input");
                resp.StatusCode = 422;

                */
                log.LogInformation($"critical error: {e.Message}");
                return new BadRequestObjectResult("Invalid Input") { StatusCode = 422 };
            }

            try
            {
                await policy.Execute(async () =>
                 {
                    // Call a Webservice
                    response = await _stockQuoteService.GetQuoteAsync(stockSymbol.stockSymbol, stockSymbol.licenseKey).ConfigureAwait(false);

                   //  response = null;

                    // Force a retry
                    if (response == null)
                         throw new Exception("http request failed");

                     // Handle result
                     //log.LogInformation($" result: {response.StatusCode}");
                 });
            }
            catch (Exception e)
            {
                // Can't recover at this point
                log.LogInformation($"critical error: {e.Message}");
            }

            log.LogInformation($"{response} received.");

            return (ActionResult)new OkObjectResult(XmlUtility.ObjectToXmlString(response, typeof(QuoteData)));

            //return XmlUtility.ObjectToXmlString(response, typeof(QuoteData));
            //return ObjectToXmlString_second(response, typeof(QuoteData));
        }



        [FunctionName("GetQuickQuote")]
        //public static async Task<IActionResult> Run(
        public async Task<String> GetQuickQuote(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            decimal response = await _stockQuoteService.GetQuickQuoteAsync("MSFT", "0").ConfigureAwait(false);

            log.LogInformation($"{response} received.");

            return XmlUtility.ObjectToXmlString(new Decimal(Convert.ToSingle(response)), typeof(Decimal));
        }

        /*
        [FunctionName("GetQuoteDataSet")]
        //public static async Task<IActionResult> Run(
        public async Task<String> GetQuoteDataSet(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            ArrayOfXElement response = await _stockQuoteService.GetQuoteDataSetAsync("MSFT", "0").ConfigureAwait(false);

            log.LogInformation($"{response} received.");

            return XmlUtility.ObjectToXmlString(response, typeof(ArrayOfXElement));
        }

        */
    }
}