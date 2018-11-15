﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keeper2018
{
    public static class NbRbRatesOldTxt
    {
        public static async Task<List<OfficialRates>> LoadFromOldTxtAsync()
        {
            await Task.Delay(1);
            return LoadFromOldTxt(GetMyUsdRates(), GetCbrRates()).ToList();
        }

        private static Dictionary<DateTime, double> GetCbrRates()
        {
            var content = File.ReadAllLines(DbUtils.GetTxtFullPath("cbrf.csv"), Encoding.GetEncoding("Windows-1251")).ToList();

            var result = new Dictionary<DateTime, double>();
            var delimiters = new[] { ' ', '"' };
            foreach (var line in content)
            {
                var ss = line.Split(',');
                var date = DateTime.Parse(ss[0].Trim());
                var str = ss.Length == 3 ? ss[1].Trim(delimiters) + ss[2].Trim(delimiters) : ss[1].Trim(delimiters);
                var value = double.Parse(str, NumberStyles.Any, new CultureInfo("en-US"));
                result.Add(date, value);
            }
            return result;
        }

        private static Dictionary<DateTime, double> GetMyUsdRates()
        {
            var content = File.ReadAllLines(DbUtils.GetTxtFullPath("CurrencyRates.txt"), Encoding.GetEncoding("Windows-1251")).
                Where(s => !String.IsNullOrWhiteSpace(s) && s.Contains("; BY")).ToList();

            var result = new Dictionary<DateTime, double>();
            foreach (var line in content)
            {
                var ss = line.Split(';');
                var date = DateTime.Parse(ss[0].Trim());
                if (date < new DateTime(2016, 7, 1))
                {
                    if (ss[1].Trim() == "BYN") continue;
                }
                else
                {
                    if (ss[1].Trim() == "BYR") continue;
                }

                var value = double.Parse(ss[2], new CultureInfo("en-US"));
                if (date < new DateTime(2000, 1, 1))
                    value = value * 1000;
                result.Add(date, value);
            }
            return result;
        }

        private static IEnumerable<OfficialRates> LoadFromOldTxt(Dictionary<DateTime, double> myUsdRates, Dictionary<DateTime, double> cbrRates)
        {
            var content = File.ReadAllLines(DbUtils.GetTxtFullPath("OfficialRates.txt"), Encoding.GetEncoding("Windows-1251")).
                Where(s => !String.IsNullOrWhiteSpace(s)).ToList();

            // 1-April-1995
            var currentRate = 11550.0;
            var currentCbrRate = 4897.0;

            foreach (var line in content)
            {
                var oneDay = NbRbRateFromString(line);

                if (myUsdRates.ContainsKey(oneDay.Date))
                    currentRate = myUsdRates[oneDay.Date];

                if (cbrRates.ContainsKey(oneDay.Date))
                    currentCbrRate = cbrRates[oneDay.Date];

                oneDay.MyUsdRate.Unit = 1;
                oneDay.MyUsdRate.Value = currentRate;

                oneDay.CbrRate.Usd.Unit = 1;
                oneDay.CbrRate.Usd.Value = currentCbrRate;

                yield return (oneDay);
            }
        }

        private static OfficialRates NbRbRateFromString(string str)
        {
            var rate = new OfficialRates();
            var substrings = str.Split(';');
            rate.Date = Convert.ToDateTime(substrings[0], new CultureInfo("ru-RU"));

            rate.NbRates.Usd.Value = Convert.ToDouble(substrings[1], new CultureInfo("en-US")) / GetDenominator(rate.Date);
            rate.NbRates.Euro.Value = Convert.ToDouble(substrings[2], new CultureInfo("en-US")) / GetDenominator(rate.Date);
            rate.NbRates.Rur.Unit = rate.Date < new DateTime(2016, 7, 1) ? 1 : 100;
            rate.NbRates.Rur.Value = Convert.ToDouble(substrings[3], new CultureInfo("en-US")) / GetDenominator(rate.Date) * rate.NbRates.Rur.Unit;

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