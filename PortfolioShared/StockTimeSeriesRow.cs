using System;
using System.Collections.Generic;

namespace PortfolioShared
{
    public class StockTimeSeriesRow
    {
        public DateTime Date { get; set; }
        public Dictionary<string, decimal> Prices { get; set; } = new();
    }
}
