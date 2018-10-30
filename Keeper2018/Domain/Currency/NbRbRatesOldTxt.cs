using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Keeper2018
{
    public static class NbRbRatesOldTxt
    {
        public static IEnumerable<NbRbRate> LoadFromOldTxt()
        {
            var content = File.ReadAllLines(DbUtils.GetTxtFullPath("OfficialRates.txt"), Encoding.GetEncoding("Windows-1251")).
                Where(s => !String.IsNullOrWhiteSpace(s)).ToList();
            foreach (var line in content)
            {
                var oneDay = NbRbRateFromString(line);
                yield return (oneDay);
            }
        }

        private static NbRbRate NbRbRateFromString(string str)
        {
            var rate = new NbRbRate();
            var substrings = str.Split(';');
            rate.Date = Convert.ToDateTime(substrings[0], new CultureInfo("ru-RU"));

            rate.Usd.Value = Convert.ToDouble(substrings[1], new CultureInfo("en-US")) / GetDenominator(rate.Date);
            rate.Euro.Value = Convert.ToDouble(substrings[2], new CultureInfo("en-US")) / GetDenominator(rate.Date);
            rate.Rur.Unit = rate.Date < new DateTime(2016, 7, 1) ? 1 : 100;
            rate.Rur.Value = Convert.ToDouble(substrings[3], new CultureInfo("en-US")) / GetDenominator(rate.Date) * rate.Rur.Unit;

            return rate;
        }

        private static double GetDenominator(DateTime date)
        {
            if (date < new DateTime(2000, 1, 1))
                return 0.001;

            if (date < new DateTime(2016, 7, 1))
                return 1;

            return 10000;
        }
        
    }
}