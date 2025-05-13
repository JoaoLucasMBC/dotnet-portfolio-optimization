using System;
using System.Collections.Generic;

namespace PortfolioShared
{
    // This class represents a row in a time series of stock prices.
    // It contains a date and a dictionary of stock prices for that date.
    // It is passed to the F# script for the simulation process.
    public class StockTimeSeriesRow
    {
        public DateTime Date { get; set; }
        public Dictionary<string, decimal> Prices { get; set; } = new();
    }
}
