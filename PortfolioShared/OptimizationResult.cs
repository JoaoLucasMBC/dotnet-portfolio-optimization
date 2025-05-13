using MathNet.Numerics.LinearAlgebra;

namespace PortfolioShared
{
    // This class represents the result of an optimization process.
    // It contains a list of stock tickers, the Sharpe ratio, and the weights of the stocks in the portfolio.
    // It is passed back to C# from the F# script after the optimization process is complete.
    public record OptimizationResult
    {
        public List<string> Tickers { get; init; }
        public double Sharpe { get; init; }
        public List<double> Weights { get; init; }
    }
}
