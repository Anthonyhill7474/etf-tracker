using System;
using System.Linq;

/// <summary>
/// Provides RSI (Relative Strength Index) calculation functionality.
/// </summary>
public static class RSIHelper
{
    /// <summary>
    /// Calculates the RSI for a given array of closing prices and period.
    /// </summary>
    /// <param name="prices">Array of closing prices</param>
    /// <param name="period">Number of periods to calculate over (e.g. 14)</param>
    /// <returns>RSI value between 0–100</returns>
    public static decimal CalculateRSI(decimal[] prices, int period)
    {
        if (prices.Length <= period)
            return 50;

        decimal gain = 0, loss = 0;
        var recent = prices.Skip(prices.Length - (period + 1)).ToArray();

        for (int i = 1; i <= period; i++)
        {
            var delta = recent[i] - recent[i - 1];
            if (delta > 0) gain += delta;
            else loss -= delta;
        }

        if (loss == 0) return 100;
        if (gain == 0) return 0;

        decimal rs = gain / period / (loss / period);
        return 100 - (100 / (1 + rs));
    }
}
