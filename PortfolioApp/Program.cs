using PortfolioRules;

class Program
{
    static void Main(string[] args)
    {
        var path = Path.Combine("data", "dow_jones_close_prices_aug_dec_2024.csv");
        var loader = new CsvTimeSeriesLoader();
        var data = loader.Load(path);

        var results = Optimizer.optimize(data); // `data` is List<StockTimeSeriesRow>
        
        var outputPath = Path.Combine("data", "output.csv");
        CsvWriter.WriteToCsv(outputPath, results);

        // Get the max Sharpe ratio and its corresponding weights
        var maxSharpe = results.Max(r => r.Sharpe);
        var maxSharpeResult = results.First(r => r.Sharpe == maxSharpe);

        Console.WriteLine("Max Sharpe Ratio: " + maxSharpe);
        Console.WriteLine("Weights: " + string.Join(", ", maxSharpeResult.Weights));
        Console.WriteLine("Tickers: " + string.Join(", ", maxSharpeResult.Tickers));
    }
}
