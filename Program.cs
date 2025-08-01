using dotenv.net;
using System;
using System.Threading.Tasks;
using Services;

class Program
{
    static async Task Main(string[] args)
    {
        DotEnv.Load();
        string[] etfs = { "SPY", "VOO", "VTI", "SMH", "QQQM", "VOOG", "SPMO", "CLOU", "BOTZ", "ARKK", "IGV", "SKYY", "SOXX", "XSD"};

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
