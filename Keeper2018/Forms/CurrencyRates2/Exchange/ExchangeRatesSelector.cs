using System;
using System.Collections.Generic;
using KeeperDomain.Exchange;

namespace Keeper2018
{
    public static class ExchangeRatesSelector
    {
        public static List<ExchangeRates> SelectMiddayRates(List<ExchangeRates> bnb, DateTime date)
        {
            var id = 0;
            var prev = bnb[0];
            date = date.Date.AddHours(12);

            var bnbD = new List<ExchangeRates>();
            foreach (var line in bnb)
            {
                while (line.Date > date)
                {
                    if (prev.Date.Day != date.Day)
                        prev.Date = date.Date;
                    var item = prev.Clone();
                    item.Id = ++id;
                    item.Date = item.Date.Date; // очистить часы минуты
                    bnbD.Add(item);
                    date = date.AddDays(1);
                }

                prev = line;

            }

            return bnbD;
        }
    }
}
