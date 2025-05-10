using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using PortfolioShared;

public class CsvTimeSeriesLoader
{
    public List<StockTimeSeriesRow> Load(string path)
    {
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        });

        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord!;
        var stocks = headers[1..];

        var result = new List<StockTimeSeriesRow>();

        while (csv.Read())
        {
            var row = new StockTimeSeriesRow
            {
                Date = csv.GetField<DateTime>("Date")
            };

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
