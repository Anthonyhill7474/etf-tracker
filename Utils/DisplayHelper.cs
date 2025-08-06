using System;
using System.Threading.Tasks;
using Utils;
using System.Text;

/// <summary>
/// Displays ETF analysis to the console and compiles result into a summary.
/// </summary>
public static class DisplayHelper
{
    /// <summary>
    /// Prints RSI interpretation and thresholds for users.
    /// </summary>
    public static void PrintRSIExplanation()
    {
        Console.WriteLine("=== RSI (Relative Strength Index) Interpretation ===");
        Console.WriteLine("RSI is a momentum indicator that measures recent price changes to evaluate overbought or oversold conditions.");
        Console.WriteLine("RSI > 70 = Overbought | RSI < 30 = Oversold\n");
        Console.WriteLine("Periods: 14-day (short), 30-day (medium), 60-day (long)");
        Console.WriteLine("Dip Candidate: Drop > 5% and RSI < 42");
        Console.WriteLine("Long-Term Dip: Drop > 8% and RSI < 44\n");
    }

    /// <summary>
    /// Prints and appends detailed analysis of a given ETF including drop %, RSI, and dip conditions.
    /// Updates watchlists and dip candidate lists.
    /// </summary>
    /// <param name="symbol">ETF symbol</param>
    /// <param name="closes30">Array of last 30 closing prices</param>
    /// <param name="closes90">Array of last 90 closing prices</param>
    /// <param name="summary">Shared summary text object</param>
    /// <param name="dipCandidates">List of dip candidates</param>
    /// <param name="shortTermWatchlist">List of short-term drop watch symbols</param>
    /// <param name="longTermWatchlist">List of long-term drop watch symbols</param>
    public static async Task PrintETFAnalysis(string symbol, decimal[] closes30, decimal[] closes90, StringBuilder? summary = null, List<string>? dipCandidates = null, List<string>? shortTermWatchlist = null, List<string>? longTermWatchlist = null, List<string>? dropSummaries = null)
    {
        decimal latest = closes30.Last();
        decimal high30 = closes30.Max();
        decimal drop30 = (1 - (latest / high30)) * 100;
        decimal rsi30 = RSIHelper.CalculateRSI(closes30, 14);

        decimal high90 = closes90.Max();
        decimal drop90 = (1 - (latest / high90)) * 100;
        decimal rsi60 = RSIHelper.CalculateRSI(closes90, 60);

        string line1 = $"{symbol}: ${latest} | 30d high ${high30} | Drop {drop30:F2}% | RSI: {rsi30:F1}";
        string line2 = $"{symbol} (3m): ${latest} | 90d high ${high90} | Drop {drop90:F2}% | RSI: {rsi60:F1}";

        Console.WriteLine(line1);
        Console.WriteLine(line2);
        dropSummaries.Add($"{symbol}: 30d {drop30:F2}%, 90d {drop90:F2}%");
        summary?.AppendLine(line1).AppendLine(line2);

        if (drop30 > 4)
        {
            shortTermWatchlist?.Add(symbol);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠️ {symbol} dropped {drop30:F2}% from 30d high");
            summary?.AppendLine($"⚠️ {symbol} dropped {drop30:F2}% from 30d high");

            string subject = $"⚠️ DROP ALERT: {symbol} 30d Drop {drop30:F2}%";
            string body = $"{symbol} has dropped {drop30:F2}% from 30-day high.\nPrice: {latest}, High: {high30}";

            if (rsi30 < 42)
            {
                dipCandidates?.Add(symbol);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✅ {symbol} is a DIP CANDIDATE (Drop > 5%, RSI < 42)");
                summary?.AppendLine($"✅ {symbol} is a DIP CANDIDATE (Drop > 5%, RSI < 42)");

                subject = $"✅ DIP CANDIDATE: {symbol}";
                body += $"\nRSI: {rsi30:F1} (oversold)";
            }
            else
            {
                body += $"\nRSI: {rsi30:F1} (not oversold)";
            }

            // await EmailHelper.SendEmailAsync(subject, body);
            Console.ResetColor();
        }
        else if (drop90 > 7)
        {
            longTermWatchlist?.Add(symbol);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"🔍 {symbol} dropped {drop90:F2}% from 90d high");
            summary?.AppendLine($"🔍 {symbol} dropped {drop90:F2}% from 90d high");

            string subject = $"🔍 LONG-TERM DROP: {symbol} 90d Drop {drop90:F2}%";
            string body = $"{symbol} has dropped {drop90:F2}% from 90-day high.\nPrice: {latest}, High: {high90}";

            if (rsi60 < 44)
            {
                dipCandidates?.Add(symbol);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✅ {symbol} is a LONG-TERM DIP CANDIDATE (Drop > 8%, RSI < 44)");
                summary?.AppendLine($"✅ {symbol} is a LONG-TERM DIP CANDIDATE (Drop > 8%, RSI < 44)");

                subject = $"✅ LONG-TERM DIP: {symbol}";
                body += $"\nRSI: {rsi60:F1} (oversold)";
            }
            else
            {
                body += $"\nRSI: {rsi60:F1} (not oversold)";
            }

            // await EmailHelper.SendEmailAsync(subject, body);
            Console.ResetColor();
        }

        summary?.AppendLine("--------------------------------");
    }
}
