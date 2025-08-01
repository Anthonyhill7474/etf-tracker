using System;
using System.Threading.Tasks;
using Utils;
using Services;

namespace ETFTracker.Tests
{
    class DipAlertTest
    {
        public static async Task RunTest()
        {
            string ticker = "SPY"; // Replace with another ETF symbol if needed
            string apiKey = Environment.GetEnvironmentVariable("TWELVE_DATA_API_KEY")!;
            
            try
            {
                var (latest, high, drop, rsi) = await DataFetcher.GetHistoricalDataForRSI(ticker, apiKey, 30);

                Console.WriteLine($"{ticker}: Drop {drop:F2}% | RSI: {rsi:F1}");

                if (drop > 5 && rsi < 35)
                {
                    Console.WriteLine("✅ Dip criteria met → sending test email");
                    await EmailHelper.SendEmailAsync(
                        subject: $"Dip Alert: {ticker}",
                        body: $"{ticker} has dropped {drop:F2}% with RSI {rsi:F1}"
                    );
                }
                else
                {
                    Console.WriteLine("❌ Dip criteria not met");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during dip test: {ex.Message}");
            }
        }
    }
}
