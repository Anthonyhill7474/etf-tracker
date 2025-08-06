using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

/// <summary>
/// Retrieves and displays the latest VIX (Volatility Index) value from the FRED API.
/// </summary>
public static class VixService
{
    /// <summary>
    /// Fetches and displays the latest available VIX data using the FRED API.
    /// </summary>
    /// <param name="fredKey">FRED API key</param>
    public static async Task <string> GetVixSummary(string fredKey)
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
                return $"📈 VIX as of {obs["date"]}: {vix:F2}\n";
            }
        }

        return "⚠️ VIX data unavailable\n";
    }
}
