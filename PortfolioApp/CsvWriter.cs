using MathNet.Numerics.LinearAlgebra;
using PortfolioShared;


public class CsvWriter
{
    public static void WriteToCsv(string path, IEnumerable<OptimizationResult> results)
    {
        using var writer = new StreamWriter(path);

        // Header: T1,...,T25,W1,...,W25,Sharpe
        var tickerHeaders = Enumerable.Range(1, 25).Select(i => $"T{i}");
        var weightHeaders = Enumerable.Range(1, 25).Select(i => $"W{i}");
        writer.WriteLine(string.Join(",", tickerHeaders.Concat(weightHeaders)) + ",Sharpe");

        // Rows
        foreach (var result in results)
        {
            var tickers = result.Tickers.ToArray();
            var weights = result.Weights.Row(0).ToArray();
            var sharpe = result.Sharpe;

            // Tickers
            var tickerRow = string.Join(",", tickers);

            // Weights
            var weightRow = string.Join(",", weights);

            // Write the row
            writer.WriteLine($"{tickerRow},{weightRow},{sharpe}");
        }
    }
}