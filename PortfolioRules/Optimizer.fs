namespace PortfolioRules

open PortfolioShared
open System.Collections.Generic
open System

open Sharpe
open MathUtils

module Optimizer =

    let toPriceMatrix (data: List<StockTimeSeriesRow>) (allTickers: string list) : float[][] =
        data
        |> Seq.toList
        |> List.map (fun row ->
            allTickers |> List.map (fun ticker -> float row.Prices.[ticker]) |> List.toArray
        )
        |> List.toArray

    let toReturnMatrix (priceMatrix: float[][]) : float[][] =
        priceMatrix
        |> Array.pairwise
        |> Array.map (fun (prev, curr) ->
            Array.map2 (fun pPrev pCurr -> pCurr / pPrev - 1.0) prev curr
        )

    let findBestSharpeRatio (dailyReturns: float[][]) (indices: int list) (covarianceMatrix: float[][]) =
        let filteredMatrix = extractColumns dailyReturns indices
        let filteredCovarianceMatrix = extractColumns covarianceMatrix indices

        let results =
            Array.Parallel.init 1000 (fun _ ->
                let weights = generateWeights indices.Length
                let sharpe = calculateSharpeRatio filteredMatrix weights filteredCovarianceMatrix
                (sharpe, weights)
            )

        results |> Array.maxBy fst

    let optimize (data: List<StockTimeSeriesRow>) =
        let allTickers = data[0].Prices.Keys |> Seq.toList
        let tickerIndexMap = allTickers |> List.mapi (fun i t -> t, i) |> dict

        let tickersCombinations = combine allTickers 25 |> List.toArray

        printfn "Number of combinations: %d" (tickersCombinations.Length)

        let priceMatrix = toPriceMatrix data allTickers
        let returnMatrix = toReturnMatrix priceMatrix

        let covarianceMatrix = covariance returnMatrix

        let results =
            tickersCombinations
            |> Array.map (fun tickers ->
                let indices = tickers |> List.map (fun t -> tickerIndexMap[t])
                let sharpe, weights = findBestSharpeRatio returnMatrix indices covarianceMatrix
                OptimizationResult(
                    Tickers = System.Collections.Generic.List<string>(tickers),
                    Sharpe = sharpe,
                    Weights = System.Collections.Generic.List<double>(weights)
                )
            )

        results