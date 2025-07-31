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
using YahooFinanceApi;
using System.Security.Cryptography;



class Program
{

    static async Task Main(string[] args)
    {
        DotEnv.Load();

        string? apiKey = Environment.GetEnvironmentVariable("FINNHUB_API_KEY");

        //to add to
        // string ETFs = "QQQM,VOOG,SPMO,SMH,SPY,VOO,VTI";
        string[] etfs = {"SPY", "VOO", "VTI", "SMH", "QQQM", "VOOG", "SPMO"};

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
        
        // Debug: Show API key lengths (first few characters for verification)
        Console.WriteLine($"Finnhub API Key: {apiKey.Substring(0, Math.Min(10, apiKey.Length))}... (length: {apiKey.Length})");
        Console.WriteLine($"Twelve Data API Key: {twelveApiKey.Substring(0, Math.Min(10, twelveApiKey.Length))}... (length: {twelveApiKey.Length})");
        Console.WriteLine();
        
        // Test API keys with simple endpoints first
        await TestAPIs(apiKey, twelveApiKey);
        
        string? polygonApiKey = Environment.GetEnvironmentVariable("POLYGON_KEY");
        // var (vix, vixChange, vixTrend) = await GetVIXTrend_Polygon(polygonApiKey);
        
        var (vix, date) = await GetTodayVix();
        if (vix.HasValue)
            Console.WriteLine($"📈 VIX as of {date}: {vix:F2}");
        else
            Console.WriteLine("No VIX data returned.");

        
        foreach (string symbol in etfs)
        {
            string trimmedSymbol = symbol.Trim();
            try
            {
                // need to wait on free plan, 5 calls per minute
                await Task.Delay(13000); // 13 seconds between calls (max 4/min)
                var ETFcloses = await GetLast30Closes(trimmedSymbol);
                if (ETFcloses.Length < 14)
                {
                    Console.WriteLine($"{symbol}: Insufficient data\n");
                    continue;
                }
                
                decimal latest = ETFcloses.Last();
                decimal high = ETFcloses.Max();
                decimal drop = (1 - (latest / high)) * 100;
                decimal rsi = CalculateRSI(ETFcloses);
                System.Console.WriteLine($"{trimmedSymbol}: ${latest} | 30d high ${high} | Drop {drop:F2}% | RSI: {rsi:F1}");
                if (drop > 5 && rsi < 35)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✅ {trimmedSymbol} is a DIP CANDIDATE (Drop > 5%, RSI < 35\n");
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

    static async Task<decimal[]> GetLast30Closes(string symbol)
    {
        string? apiKey = Environment.GetEnvironmentVariable("ALPHA_VANTAGE_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("❌ Alpha Vantage API key not found.");
            return Array.Empty<decimal>();
        }

        try
        {
            var url = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&outputsize=compact&apikey={apiKey}";
            using var client = new HttpClient();
            
            Console.WriteLine($"\n=== Fetching historical data for {symbol} from Alpha Vantage ===");
            Console.WriteLine($"URL: {url}");

            var response = await client.GetStringAsync(url);
            var json = JObject.Parse(response);

            var timeSeries = json["Time Series (Daily)"] as JObject;
            if (timeSeries == null)
            {
                Console.WriteLine("❌ No time series data found.");
                return Array.Empty<decimal>();
            }

            var closes = timeSeries.Properties()
                .OrderByDescending(p => DateTime.Parse(p.Name)) // Most recent first
                .Take(30)
                .Select(p => decimal.Parse(p.Value["4. close"]!.ToString()))
                .Reverse() // Reverse to make it oldest → newest
                .ToArray();

            Console.WriteLine($"✅ {closes.Length} close prices retrieved.");
            return closes;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❗ Error fetching data for {symbol}: {ex.Message}");
            return Array.Empty<decimal>();
        }
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

    // static async Task<(decimal current, decimal change, string trend)> GetVIXTrend_Polygon(string apiKey)
    // {
    //     using var client = new HttpClient();
    //     string end = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
    //     string start = DateTime.UtcNow.Date.AddDays(-7).ToString("yyyy-MM-dd");
    //     string url = $"https://api.polygon.io/v2/aggs/ticker/VIX/range/1/day/{start}/{end}?apiKey={apiKey}";
    //     Console.WriteLine($"Fetching VIX from Polygon: {url}");

    //     var jsonText = await client.GetStringAsync(url);
    //     var json = JObject.Parse(jsonText);

    //     if (json["results"] == null || !json["results"].HasValues)
    //         return (0, 0, "No data");

    //     var arr = json["results"].ToObject<List<JObject>>();
    //     arr = arr.OrderBy(r => DateTime.Parse(r["t"]!.ToString())).ToList();

    //     decimal first = arr.First()["c"]!.Value<decimal>();
    //     decimal last = arr.Last()["c"]!.Value<decimal>();
    //     decimal change = last - first;
    //     string trend = change > 0 ? "Bearish (VIX rising)" : "Bullish (VIX falling)";
    //     return (last, change, trend);
    // }

    static async Task<(decimal? value, string date)> GetTodayVix()
    {
        string? apiKey = Environment.GetEnvironmentVariable("FRED_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("❌ FRED API key not found.");
            return (null, "");
        }

        string url = $"https://api.stlouisfed.org/fred/series/observations?series_id=VIXCLS&api_key={apiKey}&file_type=json&sort_order=desc&limit=1";

        using var client = new HttpClient();
        var response = await client.GetStringAsync(url);
        var json = JObject.Parse(response);

        var obs = json["observations"]?.FirstOrDefault();
        if (obs == null || obs["value"] == null || obs["value"]?.ToString() == ".")
            return (null, "");

        decimal vix = decimal.Parse(obs["value"]!.ToString());
        string date = obs["date"]!.ToString();

        return (vix, date);
    }




    static string VixComment(decimal vix)
    {
        string vixComment = "Vix index is ";
        if (vix <= 15) return vixComment + "Low (Optimism)";
        if (vix <= 20) return vixComment + "Moderate (Normal market environment)";
        if (vix <= 25) return vixComment + "Medium (Growing concern)";
        if (vix <= 30) return vixComment + "High (Turbulence)";
        return vixComment + "Extreme (Market panic)";
    }


    static async Task TestAPIs(string finnhubKey, string twelveDataKey)
    {
        Console.WriteLine("=== Testing API Keys ===");
        
        // Test Finnhub with a simple quote endpoint
        try
        {
            using var client = new HttpClient();
            var url = $"https://finnhub.io/api/v1/quote?symbol=SPY&token={finnhubKey}";
            Console.WriteLine("Testing Finnhub quote endpoint...");
            var response = await client.GetStringAsync(url);
            var json = JObject.Parse(response);
            Console.WriteLine($"Finnhub Response: {json}");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Finnhub Quote API Error: {ex.Message}");
            Console.WriteLine($"Status Code: {ex.StatusCode}");
        }
        
        // Test Twelve Data with a simple quote endpoint
        try
        {
            using var client = new HttpClient();
            var url = $"https://api.twelvedata.com/quote?symbol=SPY&apikey={twelveDataKey}";
            Console.WriteLine("Testing Twelve Data quote endpoint...");
            var response = await client.GetStringAsync(url);
            var json = JObject.Parse(response);
            Console.WriteLine($"Twelve Data Response: {json}");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Twelve Data Quote API Error: {ex.Message}");
            Console.WriteLine($"Status Code: {ex.StatusCode}");
        }
        
        Console.WriteLine("=== End API Tests ===\n");
    }
}