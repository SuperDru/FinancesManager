using System.Collections.Generic;

namespace FinanceManagement.Common
{
    public class PivotPoints
    {
        private (int Left, int Right) HighPivotParameters { get; }
        private (int Left, int Right) LowPivotParameters { get; }
        
        private List<Candle> Candles { get; }
        private List<Candle> HighPivots { get; }
        private List<Candle> LowPivots { get; }

        private Candle High { get; set; }
        private int HighCount { get; set; }

        public PivotPoints(int highLeft = 15, int highRight = 15, int lowLeft = 15, int lowRight = 15)
        {
            HighPivotParameters = (highLeft, highRight);
            LowPivotParameters = (lowLeft, lowRight);

            Candles = new List<Candle>();
        }

        public void Push(Candle candle)
        {
            Candles.Add(candle);

            if (High == null || candle.High > High.High)
            {
                High = candle;
                HighCount = 0;
                return;
            }

            if (++HighCount == HighPivotParameters.Left)
            {
                HighPivots.Add(High);
                High = null;
            }
        }
    }
}