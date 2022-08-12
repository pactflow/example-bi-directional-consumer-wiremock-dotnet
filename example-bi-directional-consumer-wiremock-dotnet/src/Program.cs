using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Provider.Models;
using Newtonsoft.Json;

namespace Consumer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var baseUri = "http://localhost:9000/";

            Console.WriteLine("Fetching products");
            var consumer = new ProductClient();
            var result = await consumer.GetProducts(baseUri);
            Console.WriteLine(JsonConvert.SerializeObject(result));

            Product productResult = await consumer.GetProduct(baseUri, 10);
            Console.WriteLine(JsonConvert.SerializeObject(productResult));
        }

        static private void WriteoutArgsUsed(string datetimeArg, string baseUriArg)
        {
            Console.WriteLine($"Running consumer with args: dateTimeToValidate = {datetimeArg}, baseUri = {baseUriArg}");
        }

        static private void WriteoutUsageInstructions()
        {
            Console.WriteLine("To use with your own parameters:");
            Console.WriteLine("Usage: dotnet run ");
            Console.WriteLine("Usage Example: dotnet run 01/01/2018 http://localhost:9000");
        }
    }
}