namespace PortfolioRules

open System

module MathUtils = 

    /// Generates all combinations of k elements from a list
    let combine (tickers: string list) (k: int) : string list list =
        let rec _combine (acc: string list list) (current: string list) (remaining: string list) =
            match remaining with
            | [] -> if current.Length = k then current :: acc else acc
            | x::xs ->
                let withX = _combine acc (x::current) xs
                let withoutX = _combine withX current xs
                withoutX
        _combine [] [] tickers

    /// Normalizes a float array to sum to 1
    let normalize (arr: float[]) =
        let sum = Array.sum arr
        if sum = 0.0 then arr else Array.map (fun x -> x / sum) arr

    /// Dot product of two float arrays
    let dot (a: float[]) (b: float[]) =
        Array.map2 (*) a b |> Array.sum

    /// Mean of a float array
    let mean (xs: float[]) =
        Array.sum xs / float xs.Length

    /// Transposes a matrix (2D array)
    let transpose (matrix: float[][]) =
        Array.init matrix.[0].Length (fun j ->
            Array.init matrix.Length (fun i -> matrix.[i].[j])
        )

    /// Covariance matrix of centered data
    let covariance (data: float[][]) : float[][] =
        let rowCount = float data.Length
        let colCount = data.[0].Length

        let means =
            transpose data
            |> Array.map mean

        let centered =
            data
            |> Array.map (fun row ->
                Array.mapi (fun j x -> x - means.[j]) row
            )

        Array.init colCount (fun i ->
            Array.init colCount (fun j ->
                Array.sum (
                    centered |> Array.map (fun row -> row.[i] * row.[j])
                ) / (rowCount - 1.0)
            )
        )

    let generateWeights (count: int) : float[] =
        let random = Random()
        let rec loop () =
            let raw = Array.init count (fun _ -> random.NextDouble())
            let weights = normalize raw
            if Array.exists (fun w -> w > 0.2 || w = 0.0) weights then
                loop ()
            else
                weights
        loop ()


    let extractColumns (matrix: float[][]) (indices: int list) : float[][] =
        matrix
        |> Array.map (fun row -> indices |> List.map (fun i -> row.[i]) |> List.toArray)