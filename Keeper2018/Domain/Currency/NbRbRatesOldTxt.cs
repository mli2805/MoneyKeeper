using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Keeper2018
{
    public static class NbRbRatesOldTxt
    {
        public static ObservableCollection<NbRbRate> LoadFromOldTxt()
        {
            var result = new ObservableCollection<NbRbRate>();

            var content = File.ReadAllLines(DbUtils.GetTxtFullPath("OfficialRates.txt"), Encoding.GetEncoding("Windows-1251")).
                Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            foreach (var line in content)
            {
                var oneDay = NbRbRateFromString(line);
                result.Add(oneDay);
            }
            return result;
        }

        private static NbRbRate NbRbRateFromString(string str)
        {
            var rate = new NbRbRate();
            var substrings = str.Split(';');
            rate.Date = Convert.ToDateTime(substrings[0], new CultureInfo("ru-RU"));
            rate.Values.Usd = Convert.ToDouble(substrings[1], new CultureInfo("en-US"));
            rate.Values.Euro = Convert.ToDouble(substrings[2], new CultureInfo("en-US"));
            rate.Values.Rur = Convert.ToDouble(substrings[3], new CultureInfo("en-US"));
            return rate;
        }
    }
}