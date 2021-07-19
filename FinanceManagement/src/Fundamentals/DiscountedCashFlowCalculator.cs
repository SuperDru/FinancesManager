using System;

namespace FinanceManagement.Fundamentals
{
    public static class DiscountedCashFlowCalculator
    {
        public static decimal DiscountedCashFlow(
            this MarketFinancials financials, 
            decimal discountRate = 0.1m, 
            decimal terminalGrowthRate = 0.025m,
            decimal margin = 0.2m)
        {
            var result = 0m;
            var lastFreeCashFlow = financials.FreeCashFlow;
            var discountRateMultiplier = 1 + discountRate;
            for (var i = 0; i < 5; i++)
            {
                lastFreeCashFlow += lastFreeCashFlow * financials.GrowthRate;
                result += lastFreeCashFlow / discountRateMultiplier;
                discountRateMultiplier *= 1 + discountRate;
            }

            var terminalValue = lastFreeCashFlow * (1 + financials.GrowthRate) / (discountRate - terminalGrowthRate);
            result += terminalValue / discountRateMultiplier;

            return result * (1 - margin);
        }
    }
}