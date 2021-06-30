using System;
using Tinkoff.Trading.OpenApi.Models;

namespace FinancesManager.Common
{
    public static class Extensions
    {
        private const decimal Bln = 1_000_000_000m;

        public static string OnlyTime(this DateTime dateTime) => dateTime.ToString("hh:mm:ss");

        public static CandlePayload EmptyCandle(this CandlePayload last) =>
            new CandlePayload(
                last.Close,
                last.Close,
                last.Close,
                last.Close,
                0,
                last.Time.AddMinutes(1),
                last.Interval,
                last.Figi);

        public static decimal Billions(this decimal value) => value / Bln;
        public static decimal Percent(this decimal value) => value * 100;
    }
}