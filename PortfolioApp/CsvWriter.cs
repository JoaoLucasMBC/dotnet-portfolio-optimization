using PortfolioShared;

/// Utility class to write optimization results to a CSV file.
/// Each row contains tickers, weights, and the Sharpe ratio.
public class CsvWriter
{
    /// Writes a list of OptimizationResult entries to a CSV file.
    /// path: output file path
    /// results: list of portfolios with tickers, weights, and Sharpe ratios
    public static void WriteToCsv(string path, IEnumerable<OptimizationResult> results)
    {
        using var writer = new StreamWriter(path);

        // Write header: T1,...,T25,W1,...,W25,Sharpe
        var tickerHeaders = Enumerable.Range(1, 25).Select(i => $"T{i}");
        var weightHeaders = Enumerable.Range(1, 25).Select(i => $"W{i}");
        writer.WriteLine(string.Join(",", tickerHeaders.Concat(weightHeaders)) + ",Sharpe");

        // Write each result row
        foreach (var result in results)
        {
            var tickers = result.Tickers.ToArray();  // 25 tickers
            var weights = result.Weights.ToArray();  // 25 weights
            var sharpe = result.Sharpe;

            var tickerRow = string.Join(",", tickers);
            var weightRow = string.Join(",", weights);

            writer.WriteLine($"{tickerRow},{weightRow},{sharpe}");
        }
    }
}
