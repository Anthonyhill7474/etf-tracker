using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Services
{
    /// <summary>
    /// Responsible for fetching ETF data and performing analysis.
    /// </summary>
    public static class DataFetcher
    {
        /// <summary>
        /// Analyzes a list of ETF symbols using 30-day and 90-day historical data.
        /// Generates a summary, identifies dip candidates, and sends a report via email.
        /// </summary>
        /// <param name="symbols">Array of ETF symbols (e.g., SPY, QQQM)</param>
        /// <param name="apiKey">Twelve Data API key</param>
        public static async Task AnalyzeETFs(string[] symbols, string apiKey)
        {
            var summary = new StringBuilder();
            var dipCandidates = new List<string>();
            var shortTermWatchlist = new List<string>();
            var longTermWatchlist = new List<string>();
            var dropSummaries = new List<string>();


            var dipList = new List<string>();
            string fredKey = Environment.GetEnvironmentVariable("FRED_API_KEY")!;
            string vixSummary = await VixService.GetVixSummary(fredKey);        
            summary.AppendLine(vixSummary);
            summary.AppendLine("📊 ETF Analysis Summary:\n");

            foreach (string symbol in symbols)
            {
                await Task.Delay(15000);

                var closes30 = await GetCloses(symbol, apiKey, days: 30);
                var closes90 = await GetCloses(symbol, apiKey, days: 90);

                if (closes30.Length < 1 || closes90.Length < 1)
                {
                    string msg = $"{symbol}: Insufficient data\n";
                    Console.WriteLine(msg);
                    summary.AppendLine(msg);
                    continue;
                }

                // Track original length before analysis
                int before = summary.Length;

                await DisplayHelper.PrintETFAnalysis(symbol, closes30, closes90, summary, dipCandidates, shortTermWatchlist, longTermWatchlist, dropSummaries);
                System.Console.WriteLine();

            }
            var dropSummaryText = dropSummaries.Count > 0
                ? $"📉 Drops Summary: {string.Join(" | ", dropSummaries)}\n"
                : "📉 No drop data available\n";

            var dipText = dipCandidates.Count > 0
                ? $"✅ Dip Candidates: {string.Join(", ", dipCandidates)}"
                : "✅ No current dip candidates.";

            var shortWatchlistText = shortTermWatchlist.Count > 0
                ? $"⚠️ Short-Term Watchlist: {string.Join(", ", shortTermWatchlist)}"
                : "⚠️No current Short-Term ETF candidates.";

            var longWatchlistText = longTermWatchlist.Count > 0
                ? $"🔍 Short-Term Watchlist: {string.Join(", ", longTermWatchlist)}"
                : "🔍 No current Long-Term ETF candidates.";

            var finalBody = $"{dipText}\n{shortWatchlistText}\n{longWatchlistText}\n\n{dropSummaryText}\n\n{summary.ToString()}";

            await Utils.EmailHelper.SendEmailAsync("📊 ETF Summary Report", finalBody);
        }

        /// <summary>
        /// Fetches historical closing prices for a given symbol and time frame.
        /// </summary>
        /// <param name="symbol">ETF ticker symbol</param>
        /// <param name="apiKey">Twelve Data API key</param>
        /// <param name="days">Number of days (30 or 90)</param>
        /// <returns>Array of decimal closing prices</returns>
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

        /// <summary>
        /// Gets the latest price, high, percent drop, and RSI for a given ETF.
        /// </summary>
        /// <param name="ticker">ETF symbol</param>
        /// <param name="apiKey">Twelve Data API key</param>
        /// <param name="days">Timeframe for RSI (e.g., 14, 30, 60)</param>
        /// <returns>Tuple containing latest price, high, drop %, and RSI</returns>
        public static async Task<(decimal Latest, decimal High, decimal Drop, decimal RSI)> GetHistoricalDataForRSI(string ticker, string apiKey, int days)
        {
            var closes = await GetCloses(ticker, apiKey, days);
            if (closes == null || closes.Length == 0)
                throw new Exception($"No close data available for {ticker}");

            decimal latest = closes.Last();
            decimal high = closes.Max();
            decimal drop = (1 - (latest / high)) * 100;
            decimal rsi = RSIHelper.CalculateRSI(closes, days);

            return (latest, high, drop, rsi);
        }

    }

}
