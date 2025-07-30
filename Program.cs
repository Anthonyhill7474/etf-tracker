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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;



class Program
{

    static async Task Main(string[] args)
    {
        DotEnv.Load();

        string apiKey = Environment.GetEnvironmentVariable("FINNHUB_API_KEY");

        //to add to
        // string ETFs = "QQQM,VOOG,SPMO,SMH,SPY,VOO,VTI";
        string[] etfs = { "SPY", "VOO", "VTI", "SMH", "QQQM", "VOOG", "SPMO" };

        if (string.IsNullOrEmpty(apiKey))
        {
            System.Console.WriteLine("Finnhub API key not found");
            return;
        }
        
        string? twelveApiKey = Environment.GetEnvironmentVariable("TWELVE_DATA_API_KEY");
        if (string.IsNullOrEmpty(twelveApiKey))
        {
            System.Console.WriteLine("Twelve Data API key not found");
            return;
        }
        
        var (vix, vixChange, vixTrend) = await GetVIXFromTwelveData(twelveApiKey);
        Console.WriteLine($"VIX Index: {vix:F2} ({vixTrend}), 7-day change: {vixChange:+0.00;-0.00}%\n");

        foreach (string symbol in etfs)
        {
            string trimmedSymbol = symbol.Trim();
            try
            {
                var (prices, current) = await GetLast30Closes(trimmedSymbol, apiKey);
                if (prices.Length < 14) 
                {
                    Console.WriteLine($"{trimmedSymbol}: Insufficient data (need at least 14 days)\n");
                    continue;
                }
                
                decimal high = prices.Max();
                decimal dropPercent = (1 - (current / high)) * 100;
                decimal rsi = CalculateRSI(prices);
                System.Console.WriteLine($"{trimmedSymbol}: ${current} | 30d high ${high} | Drop {dropPercent:F2}% | RSI: {rsi:F1}");
                if (dropPercent > 5 && rsi < 35 && vix > 20)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✅ {trimmedSymbol} is a DIP CANDIDATE (Drop > 5%, RSI < 35, VIX > 20)\n");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine();
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"{trimmedSymbol}: API Error - {ex.Message}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{trimmedSymbol}: Error - {ex.Message}\n");
            }
        }
    }


    // static async Task<decimal> GetCurrentPrice(string symbol, string apiKey)
    // {
    //     // using declaration disposes of resources being used, this is important as this variable uses resources like sockets and memory
    //     using var client = new HttpClient();
    //     var response = await client.GetStringAsync($"https://finnhub.io/api/v1/quote?symbol={symbol}&token={apiKey}");
    //     var json = JObject.Parse(response);
    //     return json["c"]?.Value<decimal>() ?? 0;
    //     // ?. operator checks: Is json["c"] non-null? If no → it returns null without throwing an error
    //     // ?? means: If the left-hand side is null, return 0 instead.
    // }

    static async Task<(decimal[], decimal)> GetLast30Closes(string symbol, string apiKey)
    {
        using var client = new HttpClient();
        var url = $"https://finnhub.io/api/v1/stock/candle?symbol={symbol}&resolution=D&count=30&token={apiKey}";
        var res = await client.GetStringAsync(url);
        var json = JObject.Parse(res);

        if (json["c"] == null || !json["c"].HasValues) 
        {
            Console.WriteLine($"No data returned for {symbol}");
            return (Array.Empty<decimal>(), 0);
        }

        var closes = json["c"]!.Select(v => v.Value<decimal>()).ToArray();
        return (closes, closes.Last());
    }


    static decimal CalculateRSI(decimal[] closes)
    {
        int period = 14;
        decimal gain = 0, loss = 0;

        var recent = closes.Skip(closes.Length - (period + 1)).ToArray();

        for (int i = 1; i <= period; i++)
        {
            var delta = recent[i] - recent[i - 1];
            if (delta > 0) gain += delta;
            else loss -= delta; // loss is made positive
        }

        if (loss == 0) return 100;

        decimal rs = gain / period / (loss / period);
        return 100 - (100 / (1 + rs));
    }

    static async Task<(decimal current, decimal change, string trend)> GetVIXFromTwelveData(string apiKey)
    {
        try
        {
            using var client = new HttpClient();
            string url = $"https://api.twelvedata.com/time_series?symbol=^VIX&interval=1day&outputsize=7&apikey={apiKey}";
            var response = await client.GetStringAsync(url);
            var json = JObject.Parse(response);

            if (json["values"] == null || !json["values"].HasValues)
            {
                Console.WriteLine("No VIX data returned from Twelve Data API");
                return (0, 0, "Unknown");
            }

            var vixValues = json["values"]!
                .Select(v => decimal.Parse(v["close"]!.ToString()))
                .Reverse() // oldest to newest
                .ToArray();

            if (vixValues.Length < 2)
            {
                Console.WriteLine("Insufficient VIX data points");
                return (0, 0, "Unknown");
            }

            decimal oldest = vixValues.First();
            decimal latest = vixValues.Last();
            decimal change = latest - oldest;
            string trend = change > 0 ? "Bearish (VIX rising)" : "Bullish (VIX falling)";

            return (latest, change, trend);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"VIX API Error: {ex.Message}");
            return (0, 0, "API Error");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"VIX Error: {ex.Message}");
            return (0, 0, "Error");
        }
    }


    static string VixComment(decimal vix)
    {
        if (vix < 15) return "Low volatility";
        if (vix < 20) return "Stable";
        if (vix < 30) return "Elevated fear";
        return "Panic level";
    }
}