using System;
using System.Threading.Tasks;
using Utils;

public static class DisplayHelper
{
    public static void PrintRSIExplanation()
    {
        Console.WriteLine("=== RSI (Relative Strength Index) Interpretation ===");
        Console.WriteLine("RSI is a momentum indicator that measures recent price changes to evaluate overbought or oversold conditions.");
        Console.WriteLine("RSI > 70 = Overbought | RSI < 30 = Oversold\n");
        Console.WriteLine("Periods: 14-day (short), 30-day (medium), 60-day (long)");
        Console.WriteLine("Dip Candidate: Drop > 5% and RSI < 42");
        Console.WriteLine("Long-Term Dip: Drop > 8% and RSI < 44\n");
    }

    public static async Task PrintETFAnalysis(string symbol, decimal[] closes30, decimal[] closes90)
    {
        decimal latest = closes30.Last();
        decimal high30 = closes30.Max();
        decimal drop30 = (1 - (latest / high30)) * 100;
        decimal rsi30 = RSIHelper.CalculateRSI(closes30, 14);

        decimal high90 = closes90.Max();
        decimal drop90 = (1 - (latest / high90)) * 100;
        decimal rsi60 = RSIHelper.CalculateRSI(closes90, 60);

        Console.WriteLine($"{symbol}: ${latest} | 30d high ${high30} | Drop {drop30:F2}% | RSI: {rsi30:F1}");
        Console.WriteLine($"{symbol} (3m): ${latest} | 90d high ${high90} | Drop {drop90:F2}% | RSI: {rsi60:F1}");

        if (drop30 > 5)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠️ {symbol} dropped {drop30:F2}% from 30d high");

            string subject = $"⚠️ DROP ALERT: {symbol} 30d Drop {drop30:F2}%";
            string body = $"{symbol} has dropped {drop30:F2}% from 30-day high.\nPrice: {latest}, High: {high30}";

            if (rsi30 < 42)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✅ {symbol} is a DIP CANDIDATE (Drop > 5%, RSI < 42)");
                subject = $"✅ DIP CANDIDATE: {symbol}";
                body += $"\nRSI: {rsi30:F1} (oversold)";
            }
            else
            {
                body += $"\nRSI: {rsi30:F1} (not oversold)";
            }

            await EmailHelper.SendEmailAsync(subject, body);
            Console.ResetColor();
        }
        else if (drop90 > 8)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"🔍 {symbol} dropped {drop90:F2}% from 90d high");

            string subject = $"🔍 LONG-TERM DROP: {symbol} 90d Drop {drop90:F2}%";
            string body = $"{symbol} has dropped {drop90:F2}% from 90-day high.\nPrice: {latest}, High: {high90}";

            if (rsi60 < 44)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✅ {symbol} is a LONG-TERM DIP CANDIDATE (Drop > 8%, RSI < 44)");
                subject = $"✅ LONG-TERM DIP: {symbol}";
                body += $"\nRSI: {rsi60:F1} (oversold)";
            }
            else
            {
                body += $"\nRSI: {rsi60:F1} (not oversold)";
            }

            await EmailHelper.SendEmailAsync(subject, body);
            Console.ResetColor();
        }
    }
}
