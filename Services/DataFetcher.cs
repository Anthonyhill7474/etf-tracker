using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json.Linq;

public static class DataFetcher
{
    public static async Task AnalyzeETFs(string[] symbols, string apiKey)
    {
        foreach (string symbol in symbols)
        {
            await Task.Delay(15000);
            var closes30 = await GetCloses(symbol, apiKey, days: 30);
            var closes90 = await GetCloses(symbol, apiKey, days: 90);

            if (closes30.Length < 1 || closes90.Length < 1)
            {
                Console.WriteLine($"{symbol}: Insufficient data\n");
                continue;
            }

            DisplayHelper.PrintETFAnalysis(symbol, closes30, closes90);
            System.Console.WriteLine();
        }
    }

    private static async Task<decimal[]> GetCloses(string symbol, string apiKey, int days)
    {
        using var client = new HttpClient();
        string url = days == 30
            ? $"https://api.twelvedata.com/time_series?symbol={symbol}&interval=1day&outputsize=30&apikey={apiKey}"
            : $"https://api.twelvedata.com/time_series?symbol={symbol}&interval=1day&start_date={DateTime.Today.AddMonths(-3):yyyy-MM-dd}&end_date={DateTime.Today:yyyy-MM-dd}&apikey={apiKey}";

        var response = await client.GetStringAsync(url);
        var json = JObject.Parse(response);
        var values = json["values"] as JArray;

        if (values == null || values.Count == 0)
            return Array.Empty<decimal>();

        return values.Select(p => decimal.Parse(p["close"]!.ToString())).Reverse().ToArray();
    }
}
