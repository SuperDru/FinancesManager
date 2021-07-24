using System;
using System.Collections.Generic;
using System.Linq;
using FinanceManagement.Common;

namespace FinanceManagement.Bot.Backtesting
{
    public class Position
    {
        public bool Closed { get; set; }
        
        public DateTime PositionOpenedTime { get; set; }
        public decimal PositionOpenedPrice { get; set; }

        public DateTime PositionClosedTime { get; set; }
        public decimal PositionClosedPrice { get; set; }

        public decimal ProfitValue => PositionClosedPrice - PositionOpenedPrice;
        public decimal ProfitPercent => ProfitValue / PositionOpenedPrice;
    }
    
    public class BacktestContext
    {
        public List<Position> Positions { get; } = new ();
        public StrategyStatistics Statistics { get; } = new ();

        private decimal _profitSum;
        
        public void Buy(Candle candle)
        {
            var position = Positions.LastOrDefault();
            if (position is {Closed: false})
            {
                Logger.Warning("Attempt to buy when previous position is not closed");
                return;
            }
            
            position = new Position
            {
                PositionOpenedTime = candle.Time,
                PositionOpenedPrice = candle.Close
            };
            Positions.Add(position);
        }

        public void Sell(Candle candle)
        {
            var position = Positions.LastOrDefault();
            if (position == null || position.Closed)
            {
                Logger.Warning("Attempt to sell when no opened positions");
                return;
            }
            
            position.PositionClosedTime = candle.Time;
            position.PositionClosedPrice = candle.Close;
            position.Closed = true;
            
            CalculateStatistics(position);
        }

        private void CalculateStatistics(Position position)
        {
            if (position.PositionClosedPrice > position.PositionOpenedPrice)
                Statistics.AmountOfProfitClosedPositions += 1;
            Statistics.AmountOfClosedPositions += 1;
            Statistics.ProfitValue += position.ProfitValue;
            Statistics.Profit += position.ProfitPercent;
            Statistics.MaxPositionProfit = Math.Max(Statistics.MaxPositionProfit, position.ProfitPercent);
            Statistics.MaxPositionLoss = Math.Min(Statistics.MaxPositionLoss, position.ProfitPercent);

            _profitSum += position.ProfitPercent;
            Statistics.MaxProfit = Math.Max(Statistics.MaxProfit, _profitSum);
            Statistics.MaxLoss = Math.Min(Statistics.MaxLoss, _profitSum);
        }
    }
}