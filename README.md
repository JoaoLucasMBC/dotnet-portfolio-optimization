# Dotnet Portfolio Optimizatoin

This project implements a portfolio optimizer using functional programming in F# and C#. It evaluates around 142k stock combinations, with 1000 weight combinations each, to find the one that maximizes the Sharpe Ratio for the Dow Jones stocks in the Aug-Dec 2024 period.

## **Project Description**

Using historical stock price data from the Dow Jones index, this project:

1. Calculates daily returns for each stock.
2. Generates all combinations of 25 stocks from the dataset (that contains 30 stocks).
3. For each combination, simulates 1,000 random weight allocations (considering that no weight can be 0 or greater than 0.2).
4. Computes the Sharpe Ratio for each allocation.
5. Selects the best Sharpe Ratio per combination.
6. Writes the results to a CSV file.

The optimization logic is implemented in F# and integrated with a C# application for the impure sections, such as data handling.

---

## **Installation**

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download) or higher

Clone the repository:

```bash
git clone https://github.com/JoaoLucasMBC/dotnet-portfolio-optimization.git
cd dotnet-portfolio-optimization
````

Restore dependencies:

```bash
dotnet restore
```

---

## How to Run

To run the project:

```bash
dotnet run --project PortfolioApp
```

The input CSV file should be located in the `data/` directory and named `dow_jones_close_prices_aug_dec_2024.csv`.

There is a `.py` script to download the data from Yahoo Finance. You can run it with:

```bash
python fetch_data.py
```

Remember to have the dependencies installed for that (`yfinance`).

The results will be written to `data/output.csv`, containing the best-performing stock combinations, their weights, and the sharpe.

---

## Requirements and Dependencies

### Languages

* F# (for pure functions, sharpe calculation, and parallelism)
* C# (for data loading and orchestration)

### Libraries

* `MathNet.Numerics`: for matrix operations and statistics

---

## Expected Results

The system returns the optimal weight allocation for each combination of 25 stocks that maximizes the Sharpe Ratio.

The final CSV includes:

* The Sharpe Ratio
* The list of selected tickers
* The corresponding weight for each ticker

Example output:

```csv
T1, T2, T3, ..., T25, W1, W2, W3, ..., W25, Sharpe
AAPL, MSFT, AMZN, ..., TSLA, 0.1, 0.2, 0.05, ..., 0.15, 2.72
```

Where `T1` to `T25` are the selected tickers and `W1` to `W25` are their respective weights.

---

## Performance and Optimization

This project includes the following performance enhancements:

* Daily returns are precomputed once and reused across evaluations.
* Weight evaluation is parallelized using `Array.Parallel.map`.
* Matrix slicing selects only the required columns to avoid redundant computations.

Initial runs showed a runtime of around 1h. Different computer configurations may yield different results.
