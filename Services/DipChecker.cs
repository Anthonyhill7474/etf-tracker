using System;
using System.Threading.Tasks;
using Utils;
using dotenv.net;

namespace Services
{
    /// <summary>
    /// Responsible for determining whether an ETF qualifies as a dip candidate and returning alert messages.
    /// </summary>
    public static class DipChecker
    {
        /// <summary>
        /// Evaluates whether an ETF has dropped enough with low RSI to be considered a dip candidate.
        /// </summary>
        /// <param name="symbol">ETF ticker</param>
        /// <param name="latest">Latest price</param>
        /// <param name="high">Recent high price</param>
        /// <param name="rsi">RSI value</param>
        /// <param name="isLongTerm">Whether checking long-term (90d) dip</param>
        /// <returns>Alert string or "No alert"</returns>
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

                // await EmailHelper.SendEmailAsync(subject, body);
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

                // await EmailHelper.SendEmailAsync(subject, body);
                return subject;
            }

            return "No alert";
        }
    }
}
