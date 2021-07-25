using System;

namespace FinanceManagement.Common
{
    public static class CandlesExtensions
    {
        public static DateTime StartTime(this Interval interval, DateTime time) => interval switch
        {
            Interval.Minute => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0),
            Interval.Minute3 => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute - time.Minute % 3, 0),
            Interval.Minute5 => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute - time.Minute % 5, 0),
            Interval.Minute15 => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute - time.Minute % 15, 0),
            Interval.Minute30 => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute - time.Minute % 30, 0),
            Interval.Hour => new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0),
            Interval.Hour4 => new DateTime(time.Year, time.Month, time.Day, time.Hour - time.Hour % 4, 0, 0),
            Interval.Day => new DateTime(time.Year, time.Month, time.Day, 0, 0, 0),
            Interval.Week => time.AddDays(-((7 + time.DayOfWeek - DayOfWeek.Monday) % 7)).Date,
            Interval.Month => new DateTime(time.Year, time.Month, 1, 0, 0, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(interval))
        };
    }
}