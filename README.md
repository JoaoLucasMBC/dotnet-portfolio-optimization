# Dotnet Portfolio Optimization

This project develops a portflolio optimization tool using .NET technologies,F# and C#. The goal is to maximize the Sharpe Ratio for a selection of stocks from the Dow Jones over the period Aug-Dec 2024, and analyze the use of parallelized techniques to speed up the simulations.

---

## Overview

The Dow Jones is a stock market index that tracks 30 large companies listed on stock exchanges in the United States. This project focuses on optimizing a portfolio of 25 stocks, using historical price data to compute daily returns and simulate 1000 weight allocations for each portfolio. The project achieves this goal with the following steps:

* **Enumerate combinations** of 25 stocks from the 30-stock Dow Jones index (~142k combinations).
* **Simulate 1,000 random weight allocations parallelly** for each combination.
* **Calculate the Sharpe Ratio** for each allocation.
* **Identify and record** the highest Sharpe for every combination.

The result is a CSV file listing the best combinations per portfolio by ticker, weights, and Sharpe Ratio.

---

## Project Description

This project is structured into main components:

1. **Data Acquisition and Analysis** (`data/fetch_data.py`)

   * The C# project can download the data on-demand, but the dataset can also be downloaded separately using the provided Python script.
   * Uses `yfinance` to download closing prices for a specified date range.
   * Generates two CSVs:

     * `dow_jones_close_prices_aug_dec_2024.csv` (for optimization)
     * `dow_jones_close_prices_jan_mar_2025.csv` (for testing 2025 performance on the best portfolio for 2024)

   * This folder also contains the `analysis.ipynb` notebook, which applies the best Sharpe Ratio portfolio to the 2025 dataset and see if it performs as well (spoiler: it does not). 

2. **C# Orchestration** (`PortfolioApp`)

Realizes data loading:

   * **`CsvTimeSeriesLoader.cs`**: Parses the wide-format CSV into in-memory time series.
   * **`DataDownloader.cs`**: (Optional) fetches fresh data via the Python script.

It is important to note that, to choose between the two possible loaders, there must be a `.env` file in the root of the project with the following content:

```bash
MODE="csv or api"
START_DATE="2024-08-01"
END_DATE="2024-12-31"
API_KEY="your-alpha-advantage-api-key"
```

Alpha Advantage was chosen for being a simple API to use, having a free tier. However, to be able to fetch all the data, you will need a premium account (to handle all necessary requests).

The orchestrator also writes the outputs:

   * **`CsvWriter.cs`**: Writes the optimization results to `data/output.csv`.

And has its entry point at:

   * **`Program.cs`**: Coordinates data loading, invokes the F# optimization library, and handles I/O.

3. **Pure F# Optimization Library** (`PortfolioRules`)

   * **`MathUtils.fs`**: Statistical helpers (`mean`, `dot` product, covariance calculation, etc.).
   * **`Sharpe.fs`**: Computes annualized return/volatility and calculate the Sharpe Ratio itself.
   * **`Optimizer.fs`**:

     * **Generates** all 142k unique 25-stock combinations.
     * **Produces** 1,000 valid weight vectors per combination (each weight in `(0, 0.2]`, summing to 1) parallely.
     * **Calculates** Returns the data structured in records to the orchestrator.

---

## Parallelization Strategy

* I chose F#’s `Array.Parallel.map` to distribute the 1,000 weight evaluations across multiple CPU cores.
* It was not chosen to parallelize each portfolio combination as that would have resulted in excessive overhead and memory usage, which actually makes it slower. Therefore, only the weight evaluations are parallelized.
* The program pre-computes the daily returns for all 30 stocks and the covariance matrix, and then uses **column slicing** to extract the relevant data for each portfolio combination. This approach minimizes memory overhead and redundant computations.
* On my 16-core CPU, one run takes around 18 minutes with this strategy, compared to more than 1 hour that was estimated for a sequential approach.

---

## Installation

### Prerequisites

* [.NET 9 SDK](https://dotnet.microsoft.com/download)
* [Python 3.8+](https://www.python.org/downloads/)
* `pip install -r data/requirements.txt` (installs `yfinance` and other libs)

### Setup

```bash
# Clone the repo
git clone https://github.com/JoaoLucasMBC/dotnet-portfolio-optimization.git
cd dotnet-portfolio-optimization

# Restore .NET dependencies
dotnet restore

# Install Python requirements
pip install -r data/requirements.txt
```

---

## How to Run

1. **Fetch data** (if needed):

   ```bash
   python fetch_data.py
   ```

   The details can be customized inside the script itself.

2. **Execute optimization**:

   ```bash
   dotnet run --project PortfolioApp
   ```

3. **Review output**:

   * `data/output_{timestamp}.csv`
   * To compare 2025, use the notebook `analysis.ipynb` to load the 2025 dataset and apply the best portfolio from 2024.

---

## Input / Output Formats

### Input CSV

| Column    | Description                         |
| --------- | ----------------------------------- |
| Date      | Trading date                        |
| AAPL, ... | Closing prices for each ticker (30 columns) |

### Output CSV

| Column      | Description                         |
| ----------- | ----------------------------------- |
| T1…T25     | Selected tickers of the portfolio   |
| W1…W25     | Corresponding weights (sum = 1)     |
| Sharpe     | Annualized Sharpe Ratio             |

---

## Results

> **Best Sharpe Ratio**: *3.89316*

> **Best Portfolio**: *'WMT', 'V', 'UNH', 'TRV', 'SHW', 'PG', 'NVDA', 'MMM', 'MCD', 'KO', 'JPM', 'JNJ', 'IBM', 'HON', 'HD', 'GS', 'DIS', 'CVX', 'CSCO', 'CRM', 'CAT', 'BA', 'AXP', 'AMZN', 'AAPL'*

> **Best Weights (approximation to 5 decimal places)**: *0.14793, 0.07105, 0.00675, 0.04009, 0.01429, 0.01133, 0.0166, 0.0009, 0.05736, 0.00798, 0.01426, 0.01350, 0.17414, 0.001, 0.0547, 0.02077, 0.08329, 0.00550, 0.04149, 0.08846, 0.00399, 0.02638, 0.05864, 0.03383, 0.00603*

---

## Performance Metrics

In the `log.txt` file, you can find the output and running time of 5 experiments for each type of approach: parallel and sequential. The average time comparisson was calculated and is shown below:

| Approach   | Average Time (min) |
| ---------- | ---------------- |
| Parallel   | 18.2933            |
| Sequential |  52.5512            |

---

## 2025 Performance Comparison

Running the optimizer on the **Jan–Mar 2025** dataset (`data/dow_jones_close_prices_jan_mar_2025.csv`) we found a (not so) surprising result:

    Annualized Return: -0.0394
    Annualized Volatility: 0.1615
    Sharpe Ratio: -0.2437

Past success does not guarantee future performance. The best portfolio from 2024 did not perform well in 2025, with a negative Sharpe Ratio!

---

## AI Usage

AI was used to auxiliate in the following points:

* Writing the README file.
* Documentation of the code.
* Debugging performance issues and optimizing, given that some issues were not clear to find, such as the possibility of pre-computing some aspects of the problem.

Every AI suggestion was reviewed by a human (me) before being applied.
