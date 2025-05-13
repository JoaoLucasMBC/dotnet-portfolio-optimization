namespace PortfolioRules

open System

module MathUtils = 

    // Generates all combinations of k elements from the given list.
    // tickers: list of items to combine
    // k: number of elements in each combination
    // Returns: list of all k-sized combinations
    let combine (tickers: string list) (k: int) : string list list =
        let rec _combine (acc: string list list) (current: string list) (remaining: string list) =
            match remaining with
            | [] -> if current.Length = k then current :: acc else acc
            | x::xs ->
                let withX = _combine acc (x::current) xs
                let withoutX = _combine withX current xs
                withoutX
        _combine [] [] tickers

    // Normalizes an array of floats to sum to 1.0
    // arr: input array of floats
    // Returns: array where each element is scaled by the total sum
    let normalize (arr: float[]) =
        let sum = Array.sum arr
        if sum = 0.0 then arr else Array.map (fun x -> x / sum) arr

    // Computes the dot product between two float arrays of equal length
    // a, b: input arrays
    // Returns: scalar dot product
    let dot (a: float[]) (b: float[]) =
        Array.map2 (*) a b |> Array.sum

    // Computes the mean of a float array
    // xs: input array
    // Returns: average value
    let mean (xs: float[]) =
        Array.sum xs / float xs.Length

    // Transposes a 2D matrix (array of arrays)
    // matrix: M×N matrix
    // Returns: N×M matrix (columns become rows)
    let transpose (matrix: float[][]) =
        Array.init matrix.[0].Length (fun j ->
            Array.init matrix.Length (fun i -> matrix.[i].[j])
        )

    // Computes the covariance matrix of a dataset (sample-based)
    // data: matrix of daily returns [days][assets]
    // Returns: covariance matrix [assets][assets]
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

    // Generates a single valid weight vector with values ∈ (0, 0.2]
    // count: number of weights to generate
    // Returns: normalized weight vector
    let generateWeights (count: int) : float[] =
        let random = Random()
        let rec loop () =
            let raw = Array.init count (fun _ -> random.NextDouble())
            let weights = normalize raw
            // We are looking for weights in the range (0, 0.2]
            if Array.exists (fun w -> w > 0.2 || w = 0.0) weights then
                loop ()
            else
                weights
        loop ()

    // Extracts specific columns from a matrix
    // matrix: original matrix
    // indices: list of column indices to extract
    // Returns: a new matrix with only selected columns
    let extractColumns (matrix: float[][]) (indices: int list) : float[][] =
        matrix
        |> Array.map (fun row -> indices |> List.map (fun i -> row.[i]) |> List.toArray)