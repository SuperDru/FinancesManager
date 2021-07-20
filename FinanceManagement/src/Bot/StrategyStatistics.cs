namespace FinanceManagement.Bot
{
    public class StrategyStatistics
    {
        public decimal Profit { get; set; }
        public decimal ProfitValue { get; set; }
        public decimal MaxPositionProfit { get; set; }
        public decimal MaxPositionLoss { get; set; }
        public decimal MaxProfit { get; set; }
        public decimal MaxLoss { get; set; }
        public int AmountOfClosedPositions { get; set; }
        public int AmountOfProfitClosedPositions { get; set; }
        public decimal SuccessRate => AmountOfProfitClosedPositions / (decimal) AmountOfClosedPositions;

        public override string ToString() =>
            $"Profit: {Profit:P}; " +
            $"Profit Value {ProfitValue:F4}; " +
            $"Success Rate: {SuccessRate:P}; " +
            $"Max Profit: {MaxProfit:P}; " +
            $"Max Loss: {MaxLoss:P}; " +
            $"Max Position Profit: {MaxPositionProfit:P}; " +
            $"Max Position Loss: {MaxPositionLoss:P}; " +
            $"Amount of closed positions: {AmountOfClosedPositions}";
    }
}
