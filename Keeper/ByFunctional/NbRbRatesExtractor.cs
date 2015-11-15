using System;
using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ByFunctional
{
    [Export]
    class NbRbRatesExtractor
    {
        private readonly WebClient _wc;
        private static Dictionary<string, CurrencyCodes> _currencies;

        public NbRbRatesExtractor()
        {
            _wc = new WebClient();
            _currencies = new Dictionary<string, CurrencyCodes>
            {
                {"USD", CurrencyCodes.USD}, {"EUR", CurrencyCodes.EUR}, {"RUR", CurrencyCodes.RUB}, {"RUB", CurrencyCodes.RUB}  
            };
        }

        public Dictionary<CurrencyCodes, double> GetRatesForDate(DateTime date)
        {
            var webData = _wc.DownloadString("http://www.nbrb.by/statistics/rates/ratesDaily.asp?fromdate="+date.ToString("yyyy-M-d"));
//File.WriteAllLines(@"c:\temp\nbrb.txt",new string[]{webData});
            return ParsePage(webData, date);
        }

        private static Dictionary<CurrencyCodes, double> ParsePage(string webData, DateTime date)
        {
            var result = new Dictionary<CurrencyCodes, double>();

            foreach (var key in _currencies)
            {
                var pos = webData.IndexOf(key.Key, System.StringComparison.Ordinal);                      // currency code 
                if (pos == -1) continue;
                pos = webData.IndexOf("<td>", pos, System.StringComparison.Ordinal);                      // currency name
                var startRatePos = webData.IndexOf("<td>", pos, System.StringComparison.Ordinal) + 6;     // currency rate
                var endRatePos = webData.IndexOf("</td>", startRatePos, System.StringComparison.Ordinal);
                var rateString = webData.Substring(startRatePos, endRatePos - startRatePos).Trim();
                var rateClearString = new StringBuilder();
                foreach (var symbol in rateString)
                {
                    if (Char.IsNumber(symbol)) rateClearString.Append(symbol);
                    else if (symbol == ',')
                        rateClearString.Append(CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator);
                }
                double rate;
                if (Double.TryParse(rateClearString.ToString(), out rate)) result.Add(key.Value, rate);
//                else MessageBox.Show(String.Format("{0} {1} {2}", date, key.Key, webData.Substring(pos, 100)));
            }
            return result;
        }

    }
}
