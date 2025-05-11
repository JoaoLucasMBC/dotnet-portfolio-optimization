namespace PortfolioRules

open PortfolioShared
open System.Collections.Generic
open System
open MathNet.Numerics.LinearAlgebra
open System.Threading.Tasks

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


    let generateValidWeights count =
        let rec loop acc =
            if List.length acc = 1000 then
                acc |> List.toArray
            else
                let raw = Array.init count (fun _ -> random.NextDouble())
                let weights = normalize raw
                if Array.exists (fun w -> w > 0.2 || w = 0.0) weights then
                    loop acc
                else
                    loop (weights :: acc)
        loop []

    
    let selectColumns (returns: float[][]) (indices: int list) : Matrix<float> =
        indices
        |> List.map (fun i -> returns |> Array.map (fun row -> row.[i]))
        |> List.toArray
        |> Array.transpose
        |> Matrix<float>.Build.DenseOfRowArrays



    let findBestSharpeRatio (dailyReturns: float[][]) (indices: int list) =

        let filteredMatrix = selectColumns dailyReturns indices

        let weightOptions = generateValidWeights indices.Length

        let results =
            weightOptions
            |> Array.Parallel.map (fun weights ->
                let weightVec = Matrix<float>.Build.DenseOfRowArrays([| weights |])
                let sharpe = calculateSharpeRatio filteredMatrix weightVec
                (sharpe, weightVec)
            )

        let bestSharpe, bestWeights =
            results
            |> Array.maxBy fst
        
        bestSharpe, bestWeights


    let optimize (data: List<StockTimeSeriesRow>) =

        let allTickers = data[0].Prices.Keys |> Seq.toList
        let tickerIndexMap = allTickers |> List.mapi (fun i t -> t, i) |> dict

        let tickersCombinations = combine allTickers 25 |> List.toArray
        
        printfn "Number of combinations: %d" (tickersCombinations.Length)
        
        let priceMatrix = toPriceMatrix data allTickers
        let returnMatrix = toReturnMatrix priceMatrix

        let results =
            tickersCombinations
            |> Array.Parallel.map (fun tickers ->
                let indices = tickers |> List.map (fun t -> tickerIndexMap[t])
                let sharpe, weights = findBestSharpeRatio returnMatrix indices
                OptimizationResult(
                    Tickers = System.Collections.Generic.List<string>(tickers),
                    Sharpe = sharpe,
                    Weights = weights
                )
            )

        results