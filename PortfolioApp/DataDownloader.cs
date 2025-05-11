using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using PortfolioShared;

public class DataDownloader
{
    private readonly string _apiKey;
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;

    public DataDownloader(string apiKey, DateTime startDate, DateTime endDate)
    {
        _apiKey = apiKey;
        _startDate = startDate;
        _endDate = endDate;
    }

    public List<StockTimeSeriesRow> Load(List<string> tickers)
    {
        var dataMap = new Dictionary<DateTime, Dictionary<string, decimal>>();

        using var httpClient = new HttpClient();

        foreach (var ticker in tickers)
        {
            Console.WriteLine($"Fetching data for {ticker}...");

            var url = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={ticker}&outputsize=full&apikey={_apiKey}";

            var response = httpClient.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();

            var content = response.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(content);

            if (json["Error Message"] != null)
                throw new InvalidOperationException((string)json["Error Message"]);

            if (json["Note"] != null)
                throw new InvalidOperationException((string)json["Note"]);

            if (json["Information"] != null)
                throw new InvalidOperationException((string)json["Information"]);

            var timeSeries = json["Time Series (Daily)"] as JObject;
            if (timeSeries == null)
                throw new InvalidOperationException("Time Series data not found.");

            foreach (var property in timeSeries.Properties())
            {
                if (!DateTime.TryParse(property.Name, out var date))
                    continue;

                if (date < _startDate || date > _endDate)
                    continue;

                var dailyData = property.Value;
                var closeToken = dailyData["4. close"];
                if (closeToken == null)
                    continue;

                if (!decimal.TryParse(closeToken.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var closePrice))
                    continue;

                if (!dataMap.ContainsKey(date))
                    dataMap[date] = new Dictionary<string, decimal>();

                dataMap[date][ticker] = closePrice;
            }
        }

        var result = new List<StockTimeSeriesRow>();
        foreach (var date in dataMap.Keys)
        {
            result.Add(new StockTimeSeriesRow
            {
                Date = date,
                Prices = dataMap[date]
            });
        }

        return result;
    }
}
