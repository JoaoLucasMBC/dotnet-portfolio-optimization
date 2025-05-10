namespace PortfolioRules

open PortfolioShared
open System.Collections.Generic
open System
open MathNet.Numerics.LinearAlgebra

open MathUtils

module Sharpe =    

    let calculateAnnualAverageReturn (returns: Matrix<float>) (weights: Matrix<float>) =
        let weightedReturns = returns * weights
        let dailyReturns = weightedReturns.Column(0)
        let dailyMean = dailyReturns.Sum() / float (dailyReturns.Count)
        dailyMean * 252.0


    let calculateAnuualVolatility (returns: Matrix<float>) (weights: Matrix<float>) =
        let covMatrix = covariance returns

        let portfolioVariance = (weights.Transpose() * covMatrix * weights).[0, 0]
        let portfolioStdDev = Math.Sqrt(portfolioVariance)
        
        portfolioStdDev * Math.Sqrt(252.0)

    
    let calculateSharpeRatio (dailyReturns: Matrix<float>) (weights: Matrix<float>) =

        let annualAverageReturn = calculateAnnualAverageReturn dailyReturns weights
        let annualVolatility = calculateAnuualVolatility dailyReturns weights
        
        let sharpeRatio = annualAverageReturn / annualVolatility
        
        sharpeRatio