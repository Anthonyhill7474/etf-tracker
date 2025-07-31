using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public static class VixService
{
    public static async Task ShowVix(string fredKey)
    {
        using var client = new HttpClient();
        var url = $"https://api.stlouisfed.org/fred/series/observations?series_id=VIXCLS&api_key={fredKey}&file_type=json&limit=1&sort_order=desc";
        var response = await client.GetStringAsync(url);
        var json = JObject.Parse(response);
        var obs = json["observations"]?.FirstOrDefault();

        if (obs != null && obs["value"] != null && obs["value"]?.ToString() != ".")
        {
            if (decimal.TryParse(obs["value"]!.ToString(), out var vix))
            {
                Console.WriteLine($"📈 VIX as of {obs["date"]}: {vix:F2}");
            }
            else
            {
                Console.WriteLine("⚠️ Failed to parse VIX value.");
            }
        }
    }
}
