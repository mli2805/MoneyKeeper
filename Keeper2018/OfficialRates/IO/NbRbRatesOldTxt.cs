using System;
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
            var result = new List<OfficialRates>();
            foreach (var rate in LoadFromOldTxt())
            {
                result.Add(rate);
            }
            await Task.Delay(1);
            return result;
        }

        public static IEnumerable<OfficialRates> LoadFromOldTxt()
        {
            var content = File.ReadAllLines(DbUtils.GetTxtFullPath("OfficialRates.txt"), Encoding.GetEncoding("Windows-1251")).
                Where(s => !String.IsNullOrWhiteSpace(s)).ToList();
            foreach (var line in content)
            {
                var oneDay = NbRbRateFromString(line);
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