using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using PortfolioShared;

/// Loads historical stock prices from a CSV file into a list of StockTimeSeriesRow.
/// Expected format:
/// - First column: "Date"
/// - Remaining columns: stock tickers (e.g., AAPL, MSFT, etc.)
public class CsvTimeSeriesLoader
{
    /// Loads the time series data from a CSV file.
    /// path: path to the CSV file with historical data.
    /// Returns a list of rows, each containing a date and a dictionary of stock prices.
    public List<StockTimeSeriesRow> Load(string path)
    {
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        });

        // Read and parse header
        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord!;
        var stocks = headers[1..];

        var result = new List<StockTimeSeriesRow>();

        // Read each row of the CSV
        while (csv.Read())
        {
            var row = new StockTimeSeriesRow
            {
                Date = csv.GetField<DateTime>("Date")
            };

            // Read stock prices for the row
            foreach (var stock in stocks)
            {
                if (decimal.TryParse(csv.GetField(stock), out var price))
                {
                    row.Prices[stock] = price;
                }
            }

            result.Add(row);
        }

        return result;
    }
}
