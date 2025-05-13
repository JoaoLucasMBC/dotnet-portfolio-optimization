namespace PortfolioRules

open PortfolioShared
open System.Collections.Generic
open System

open Sharpe
open MathUtils

module Optimizer =

    // Converts raw stock price data into a matrix format.
    // data: list of StockTimeSeriesRow (each row = 1 day)
    // allTickers: list of tickers in desired column order
    // Returns: matrix where rows = days, columns = tickers, values = prices
    let toPriceMatrix (data: List<StockTimeSeriesRow>) (allTickers: string list) : float[][] =
        data
        |> Seq.toList
        // Extracts prices for each ticker from the Record to craete the matrix
        |> List.map (fun row ->
            allTickers |> List.map (fun ticker -> float row.Prices.[ticker]) |> List.toArray
        )
        |> List.toArray

    // Computes the daily return matrix from the price matrix.
    // Each row represents percentage change from the previous day.
    // Drops the first row (no return can be computed for it).
    let toReturnMatrix (priceMatrix: float[][]) : float[][] =
        priceMatrix
        // The first row is dropped as it has no return
        |> Array.pairwise
        |> Array.map (fun (prev, curr) ->
            Array.map2 (fun pPrev pCurr -> pCurr / pPrev - 1.0) prev curr
        )

    // Finds the best Sharpe ratio for a given set of asset indices.
    // dailyReturns: full return matrix [days][tickers]
    // indices: list of column indices (tickers) to extract for a portfolio
    // covarianceMatrix: full covariance matrix [tickers][tickers]
    // Returns: tuple (max Sharpe ratio, corresponding weight vector)
    let findBestSharpeRatio (dailyReturns: float[][]) (indices: int list) (covarianceMatrix: float[][]) =
        // Extracts the chosen tickers columns from the daily returns matrix and covariance matrix
        let filteredMatrix = extractColumns dailyReturns indices
        let filteredCovarianceMatrix = extractColumns covarianceMatrix indices

        // Parallel computation of Sharpe ratios for 1000 random weight vectors
        let results =
            Array.init 1000 (fun _ ->
                let weights = generateWeights indices.Length
                let sharpe = calculateSharpeRatio filteredMatrix weights filteredCovarianceMatrix
                (sharpe, weights)
            )

        // Finds the maximum Sharpe ratio and its corresponding weights for the portfolio
        results |> Array.maxBy fst

    // Main optimization routine.
    // data: list of historical prices per day and ticker
    // Generates all 25-ticker combinations from Dow 30
    // Computes the Sharpe ratio for each, returning the best for each portfolio
    let optimize (data: List<StockTimeSeriesRow>) =
        // Creates a mapping of tickers to their indices in the matrix
        let allTickers = data[0].Prices.Keys |> Seq.toList
        let tickerIndexMap = allTickers |> List.mapi (fun i t -> t, i) |> dict

        // Generates all combinations of 25 tickers from the list of all tickers
        let tickersCombinations = combine allTickers 25 |> List.toArray

        // Converts the raw data into a price matrix
        let priceMatrix = toPriceMatrix data allTickers
        let returnMatrix = toReturnMatrix priceMatrix

        // Precompute full covariance matrix once
        let covarianceMatrix = covariance returnMatrix

        // Start simulation for each combination of tickers that make a portfolio
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