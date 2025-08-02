using dotenv.net;
using System;
using System.Threading.Tasks;
using Services;
using Utils;

// republish the publish file for internal script running - dotnet publish -c Release -o ./publish

class Program
{
    static async Task Main(string[] args)
    {
        DotEnv.Load();
        string[] etfs = { "SPY", "VOO", "VTI", "SMH", "QQQM", "VOOG", "SPMO", "CLOU", "BOTZ", "ARKK", "IGV", "SKYY", "SOXX", "XSD" };

        Console.WriteLine("🚀 Main started");
        Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - 🚀 Starting ETF Tracker");
        Console.Out.Flush();

        var twelveKey = Environment.GetEnvironmentVariable("TWELVE_DATA_API_KEY");
        var fredKey = Environment.GetEnvironmentVariable("FRED_API_KEY");

        if (string.IsNullOrEmpty(twelveKey) || string.IsNullOrEmpty(fredKey))
        {
            Console.WriteLine("❌ API keys missing.");
            return;
        }

        await VixService.ShowVix(fredKey);
        DisplayHelper.PrintRSIExplanation();
        await DataFetcher.AnalyzeETFs(etfs, twelveKey);
    }
}
