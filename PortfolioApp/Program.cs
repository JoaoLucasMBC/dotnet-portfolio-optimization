using PortfolioRules;
using PortfolioShared;
using DotNetEnv;
using System.Diagnostics;

/// Entry point of the application.
/// Loads stock data (via API or CSV), runs the optimizer, and writes results to a CSV file.
class Program
{
    static void Main(string[] args)
    {
        // Load environment variables from .env file
        Env.Load();

        // Get config values
        string mode = Environment.GetEnvironmentVariable("MODE")?.ToLower() ?? "csv";
        string startDateStr = Environment.GetEnvironmentVariable("START_DATE") ?? "2024-08-01";
        string endDateStr = Environment.GetEnvironmentVariable("END_DATE") ?? "2024-12-31";
        string apiKey = Environment.GetEnvironmentVariable("API_KEY") ?? string.Empty;

        if (!DateTime.TryParse(startDateStr, out DateTime startDate))
        {
            Console.WriteLine("Invalid START_DATE in .env file.");
            return;
        }

        if (!DateTime.TryParse(endDateStr, out DateTime endDate))
        {
            Console.WriteLine("Invalid END_DATE in .env file.");
            return;
        }

        List<StockTimeSeriesRow> data;

        // Load stock price data from Alpha Vantage API or CSV
        if (mode == "api")
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("API_KEY is not set in .env file.");
                return;
            }

            var tickers = new List<string> { 
                "UNH", "GS", "MSFT", "HD", "V", "SHW", "MCD", "CAT", "AMGN", "AXP",
                "TRV", "CRM", "IBM", "JPM", "AAPL", "HON", "AMZN", "PG", "BA", "JNJ",
                "CVX", "MMM", "NVDA", "WMT", "DIS", "MRK", "KO", "CSCO", "NKE", "VZ" 
            };

            var loader = new DataDownloader(apiKey, startDate, endDate);
            data = loader.Load(tickers);
        }
        else if (mode == "csv")
        {
            var path = Path.Combine("data", "dow_jones_close_prices_aug_dec_2024.csv");
            var loader = new CsvTimeSeriesLoader();
            data = loader.Load(path);
        }
        else
        {
            Console.WriteLine("Invalid MODE in .env file. Use 'csv' or 'api'.");
            return;
        }
        var sw = new Stopwatch();
        sw.Start();

        // Run optimization
        var results = Optimizer.optimize(data);

        sw.Stop();
        Console.WriteLine("Optimization took: " + sw.Elapsed.TotalMinutes + " minutes");

        // Write output to CSV with timestamped filename
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var outputPath = Path.Combine("data", $"output_{timestamp}.csv");
        CsvWriter.WriteToCsv(outputPath, results);

        // Report the best result
        var maxSharpe = results.Max(r => r.Sharpe);
        var maxSharpeResult = results.First(r => r.Sharpe == maxSharpe);

        Console.WriteLine("Max Sharpe Ratio: " + maxSharpe);
        Console.WriteLine("Tickers: " + string.Join(", ", maxSharpeResult.Tickers));
        Console.WriteLine("Weights: " + string.Join(", ", maxSharpeResult.Weights));

        Console.WriteLine("\nAll data written to " + outputPath);

    }
}
