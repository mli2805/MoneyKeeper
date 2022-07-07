using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KeeperDomain.Exchange;

namespace Keeper2018
{
    public static class ExchangeRatesSelector
    {
        // temp
        public static List<ExchangeRates> GetAllBnb()
        {
            var lines = File.ReadAllLines(@"c:\temp\KomBankRates.csv");

            var bnb = new List<ExchangeRates>();
            foreach (var line in lines.Skip(1))
            {
                var er = line.Parse();
                if (er != null)
                    bnb.Add(er);
            }

            return bnb;
        }

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
                    bnbD.Add(item);
                    date = date.AddDays(1);
                }

                prev = line;

            }   
            
            return bnbD;
        }
    }
}
