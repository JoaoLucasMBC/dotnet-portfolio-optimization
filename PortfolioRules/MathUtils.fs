namespace PortfolioRules

open MathNet.Numerics.LinearAlgebra
open System

module MathUtils = 
    let random = Random()

    let combine (tickers: string list) (k: int) : string list list =
        let rec _combine (acc: string list list) (current: string list) (remaining: string list) =
            match remaining with
            | [] -> if current.Length = k then current :: acc else acc
            | x::xs ->
                let withX = _combine acc (x::current) xs
                let withoutX = _combine withX current xs
                withoutX

        _combine [] [] tickers


    let normalize (arr: float array) =
        let sum = Array.sum arr
        arr |> Array.map (fun x -> x / sum)

    
    let covariance (data: Matrix<float>) =
        let rowCount = float data.RowCount

        // Center the data (subtract column means)
        let meanVec = data.ColumnSums() / rowCount
        let centered = data - DenseMatrix.init data.RowCount data.ColumnCount (fun i j -> meanVec.[j])

        // Covariance = (1 / (n - 1)) * (Xᵀ * X)
        (centered.TransposeThisAndMultiply(centered)) / (rowCount - 1.0)