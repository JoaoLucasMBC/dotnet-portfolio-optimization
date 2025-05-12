using MathNet.Numerics.LinearAlgebra;

namespace PortfolioShared
{
    public record OptimizationResult
    {
        public List<string> Tickers { get; init; }
        public double Sharpe { get; init; }
        public List<double> Weights { get; init; }
    }
}
