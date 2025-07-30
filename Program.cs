/*
    Program that tracks US ETFs in real time

    c - Current price
    d - Change
    dp - Percent change
    h - High price of the day
    l - Low price of the day
    o - Open price of the day
    pc - Previous close price
*/

using dotenv.net;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.VisualBasic;


class Program
{

    static async Task Main(string[] args)
    {
        DotEnv.Load();

        string apiKey = Environment.GetEnvironmentVariable("FINNHUB_API_KEY");

        //to add to
        string ETFs = "QQQM,VOOG,SPMO,SMH,SPY,VOO,VTI";

        if (string.IsNullOrEmpty(apiKey))
        {
            System.Console.WriteLine("Api key not found");
            return;
        }

        foreach (string symbol in ETFs.Split(','))
        {
            string trimmedSymbol = symbol.Trim();
            decimal currentPrice = await GetCurrentPrice(trimmedSymbol, apiKey);
            Console.WriteLine($"{trimmedSymbol}: {currentPrice}");
        }

    }

    static async Task<decimal> GetCurrentPrice(string symbol, string apiKey)
    {
        // using declaration disposes of resources being used, this is important as this variable uses resources like sockets and memory
        using var client = new HttpClient();
        var response = await client.GetStringAsync($"https://finnhub.io/api/v1/quote?symbol={symbol}&token={apiKey}");
        var json = JObject.Parse(response);
        return json["c"]?.Value<decimal>() ?? 0;
        // ?. operator checks: Is json["c"] non-null? If no → it returns null without throwing an error
        // ?? means: If the left-hand side is null, return 0 instead.
    }
}