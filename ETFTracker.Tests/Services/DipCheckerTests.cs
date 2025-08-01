using System.Threading.Tasks;
using Xunit;
using Services;
using dotenv.net;


namespace ETFTracker.Tests.Services
{
    public class DipCheckerTests
    {
        private static void LoadEnv()
        {
            var options = new DotEnvOptions(envFilePaths: new[] { "../.env" });
            DotEnv.Load(options);
        }
        [Fact]
        public async Task SendsEmailOnShortTermDip()
        {
            LoadEnv();
            string result = await DipChecker.CheckDipAndAlert("TEST", latest: 90, high: 100, rsi: 35, isLongTerm: false);
            Assert.Contains("DIP CANDIDATE", result);
        }

        [Fact]
        public async Task NoAlertWhenDropIsSmall()
        {
            LoadEnv();
            string result = await DipChecker.CheckDipAndAlert("TEST", latest: 98, high: 100, rsi: 45, isLongTerm: false);
            Assert.Equal("No alert", result);
        }
    }
}
