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
                Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
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

            var denominator = BelCurrencies.GetDenominatorForOldTxt(rate.Date);

            rate.Values.Usd.Value = Convert.ToDouble(substrings[1], new CultureInfo("en-US")) / denominator;
            rate.Values.Euro.Value = Convert.ToDouble(substrings[2], new CultureInfo("en-US")) / denominator;
            rate.Values.Rur.Value = Convert.ToDouble(substrings[3], new CultureInfo("en-US")) /denominator * 100;
            rate.Values.Rur.Unit = 100;

            return rate;
        }
    }
}