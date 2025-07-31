using System;

public static class DisplayHelper
{
    public static void PrintRSIExplanation()
    {
        Console.WriteLine("=== RSI (Relative Strength Index) Interpretation ===");
        Console.WriteLine("RSI is a momentum indicator that measures recent price changes to evaluate overbought or oversold conditions.");
        Console.WriteLine("RSI > 70 = Overbought | RSI < 30 = Oversold\n");
        Console.WriteLine("Periods: 14-day (short), 30-day (medium), 60-day (long)");
        Console.WriteLine("Dip Candidate: Drop > 5% and RSI < 35");
        Console.WriteLine("Long-Term Dip: Drop > 8% and RSI < 40\n");
    }

    public static void PrintETFAnalysis(string symbol, decimal[] closes30, decimal[] closes90)
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

        if (drop30 > 5 && rsi30 < 35)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ {symbol} is a DIP CANDIDATE\n");
        }
        else if (drop90 > 8 && rsi60 < 40)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"🔍 {symbol} is a LONG-TERM DIP CANDIDATE\n");
        }
        Console.ResetColor();
    }
}
