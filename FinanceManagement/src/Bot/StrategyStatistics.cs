using System;
using System.Collections.Generic;
using System.Linq;

namespace FinanceManagement.Bot
{
    public class StrategyStatistics
    {
        public string Ticker { get; set; }
        public string Figi { get; set; }

        public StrategyStatistics(string ticker, string figi)
        {
            Ticker = ticker;
            Figi = figi;
        }

        public decimal Profit { get; set; }
        public int AmountOfClosedPositions { get; set; }
        public int AmountOfProfitClosedPositions { get; set; }
        public decimal SuccessRate => AmountOfProfitClosedPositions / (decimal) AmountOfClosedPositions;

        public SortedDictionary<decimal, int> Positions = new SortedDictionary<decimal, int>();

        public void Long(decimal price, int size)
        {
            Positions[price] = Positions.ContainsKey(price) ? Positions[price] + size : size;
        }

        public void Short(decimal price, int size)
        {
            var remaining = size;
            var profit = 0m;

            if (Positions.Values.Sum() < size)
            {
                throw new Exception("Not enough long positions to execute short order");

            }

            foreach (var (posPrice, posSize) in Positions.ToDictionary(_ => _.Key, _ => _.Value))
            {
                if (posSize > remaining)
                {
                    profit += (price - posPrice) * remaining;
                    Positions[posPrice] -= remaining;
                    remaining = 0;
                }
                else
                {
                    profit += (price - posPrice) * posSize;
                    remaining -= posSize;
                    Positions.Remove(posPrice);
                }

                if (remaining == 0) break;
            }

            Profit += profit;

            AmountOfClosedPositions++;
            AmountOfProfitClosedPositions += profit >= 0 ? 1 : 0;
        }
        
        public override string ToString() =>
            $"Ticker: {Ticker}; Profit: {Math.Round(Profit, 4)}; Success Rate: {Math.Round(SuccessRate, 2)}; Amount of closed positions: {AmountOfClosedPositions}";
    }
}
