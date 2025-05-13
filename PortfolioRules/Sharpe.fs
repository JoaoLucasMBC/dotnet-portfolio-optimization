namespace PortfolioRules

open System
open PortfolioShared
open MathUtils

module Sharpe =

    // Computes the annualized average return of a portfolio.
    // returns: matrix of daily returns [days][assets]
    // weights: portfolio weight vector (length = number of assets)
    // Returns: expected annual return (scalar)
    let calculateAnnualAverageReturn (returns: float[][]) (weights: float[]) =
        returns
        |> Array.map (fun dayReturns -> dot dayReturns weights)  // apply weights to each day's returns
        |> mean
        |> fun dailyMean -> dailyMean * 252.0  // annualize

    // Computes the annualized volatility (standard deviation) of the portfolio.
    // returns: matrix of daily returns (unused but kept for interface symmetry)
    // weights: portfolio weights
    // cov: covariance matrix between assets
    // Returns: annualized portfolio standard deviation
    let calculateAnnualVolatility (returns: float[][]) (weights: float[]) (cov: float[][]) =
        // Does w * cov * w^T
        let weightedCov = 
            weights
            |> Array.mapi (fun i wi ->
                wi * (Array.mapi (fun j wj -> wj * cov.[i].[j]) weights |> Array.sum)
            )
            |> Array.sum
        Math.Sqrt(weightedCov) * Math.Sqrt(252.0)

    // Computes the Sharpe ratio of a portfolio.
    // returns: matrix of daily returns [days][assets]
    // weights: portfolio weights
    // covarianceMatrix: precomputed covariance matrix of the full asset set
    // Returns: Sharpe ratio (expected return / volatility)
    let calculateSharpeRatio (returns: float[][]) (weights: float[]) (covarianceMatrix: float[][]) =
        let annualReturn = calculateAnnualAverageReturn returns weights
        let volatility = calculateAnnualVolatility returns weights covarianceMatrix
        annualReturn / volatility