namespace PortfolioRules

open System
open PortfolioShared

open MathUtils

module Sharpe =

    let calculateAnnualAverageReturn (returns: float[][]) (weights: float[]) =
        returns
        |> Array.map (fun dayReturns -> dot dayReturns weights)
        |> mean
        |> fun dailyMean -> dailyMean * 252.0

    let calculateAnnualVolatility (returns: float[][]) (weights: float[]) (cov: float[][]) =
        let weightedCov = 
            weights
            |> Array.mapi (fun i wi ->
                wi * (Array.mapi (fun j wj -> wj * cov.[i].[j]) weights |> Array.sum)
            )
            |> Array.sum
        Math.Sqrt(weightedCov) * Math.Sqrt(252.0)

    let calculateSharpeRatio (returns: float[][]) (weights: float[]) (covarianceMatrix: float[][]) =
        let annualReturn = calculateAnnualAverageReturn returns weights
        let volatility = calculateAnnualVolatility returns weights covarianceMatrix
        annualReturn / volatility
