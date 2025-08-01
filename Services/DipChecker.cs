using System;
using System.Threading.Tasks;
using Utils;
using dotenv.net;

namespace Services
{
    public static class DipChecker
    {
        public static async Task<string> CheckDipAndAlert(string symbol, decimal latest, decimal high, decimal rsi, bool isLongTerm)
        {
            decimal drop = (1 - (latest / high)) * 100;
            string subject, body;
            DotEnv.Load();
            if (!isLongTerm && drop > 5)
            {
                subject = $"⚠️ DROP ALERT: {symbol}";
                body = $"{symbol} has dropped {drop:F2}% from 30-day high.\nPrice: {latest}, High: {high}\nRSI: {rsi:F1}";

                if (rsi < 42)
                {
                    subject = $"✅ DIP CANDIDATE: {symbol}";
                    body += " (RSI confirms oversold)";
                }

                await EmailHelper.SendEmailAsync(subject, body);
                return subject;
            }

            if (isLongTerm && drop > 8)
            {
                subject = $"🔍 LONG-TERM DROP: {symbol}";
                body = $"{symbol} has dropped {drop:F2}% from 90-day high.\nPrice: {latest}, High: {high}\nRSI: {rsi:F1}";

                if (rsi < 44)
                {
                    subject = $"✅ LONG-TERM DIP: {symbol}";
                    body += " (RSI confirms oversold)";
                }

                await EmailHelper.SendEmailAsync(subject, body);
                return subject;
            }

            return "No alert";
        }
    }
}
