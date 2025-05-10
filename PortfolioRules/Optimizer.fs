namespace PortfolioRules

open PortfolioShared
open System.Collections.Generic
open System
open MathNet.Numerics.LinearAlgebra
open System.Threading.Tasks

open Sharpe
open MathUtils

module Optimizer =

    let toPriceMatrix (selectedTickers: string list) (data: List<StockTimeSeriesRow>) : float[][] =
        data
        |> Seq.toList
        |> List.map (fun row ->
            selectedTickers
            |> List.map (fun ticker ->
                row.Prices.[ticker] |> float
            )
            |> List.toArray
        )
        |> List.toArray


    let generateValidWeights count =
        let rec loop acc =
            if List.length acc = 1000 then
                acc |> List.toArray
            else
                let raw = Array.init count (fun _ -> random.NextDouble())
                let weights = normalize raw
                if Array.exists (fun w -> w > 0.2) weights then
                    loop acc
                else
                    loop (weights :: acc)
        loop []

    let findBestSharpeRatio (data: List<StockTimeSeriesRow>) (tickers: string list) =
        let priceMatrix = toPriceMatrix tickers data

        // Map the price to the daily returns
        let dailyReturns =
            priceMatrix
            |> Array.pairwise
            |> Array.map (fun (prev, curr) ->
                Array.map2 (fun pPrev pCurr -> pCurr / pPrev - 1.0) prev curr
            )

        let weightOptions = generateValidWeights tickers.Length

        let dailyReturnsMat = Matrix<float>.Build.DenseOfRowArrays(dailyReturns)

        let results =
            weightOptions
            |> Array.map (fun weights ->
                let weightVec = Matrix<float>.Build.DenseOfColumnArrays([| weights |])
                let sharpe = calculateSharpeRatio dailyReturnsMat weightVec
                (sharpe, weightVec)
            )

        let bestSharpe, bestWeights =
            results
            |> Array.maxBy fst
        
        bestSharpe, bestWeights

    let optimize (data: List<StockTimeSeriesRow>) =

        let allTickers = data[0].Prices.Keys |> Seq.toList
        
        let tickersCombinations = combine allTickers 25 |> List.toArray

        printfn "Number of combinations: %d" (tickersCombinations.Length)

        let results =
            tickersCombinations
            |> Array.Parallel.map (fun tickers ->
                let sharpe, weights = findBestSharpeRatio data tickers
                OptimizationResult(
                    Tickers = System.Collections.Generic.List<string>(tickers),
                    Sharpe = sharpe,
                    Weights = weights
                )
            )

        results