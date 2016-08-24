using System;
using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using System.Net;
using System.Text;
using Keeper.DomainModel.Enumes;

namespace Keeper.Utils
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
//                {"USD", CurrencyCodes.USD}, {"EUR", CurrencyCodes.EUR}, {"RUR", CurrencyCodes.RUB}, {"RUB", CurrencyCodes.RUB}  
                {"USD", CurrencyCodes.USD}, {"EUR", CurrencyCodes.EUR},  {"RUB", CurrencyCodes.RUB}  
            };
        }

        public Dictionary<CurrencyCodes, double> GetRatesForDate(DateTime date)
        {
            var webData = _wc.DownloadString("http://www.nbrb.by/statistics/rates/ratesDaily.asp?date="+date.ToString("yyyy-MM-dd"));
            var pos = webData.IndexOf("maxDate:", StringComparison.Ordinal);
            var maxDateString = webData.Substring(pos+10, 10);
            var maxDate = DateTime.Parse(maxDateString, CultureInfo.CreateSpecificCulture("ru-Ru"));
            if (maxDate < date) return null;
                
            return ParsePage(webData);
        }

        private static Dictionary<CurrencyCodes, double> ParsePage(string webData)
        {
            var result = new Dictionary<CurrencyCodes, double>();

            foreach (var key in _currencies)
            {
                    result.Add(key.Value, GetRate(webData, key.Value.ToString()));
            }
            return result;
        }

        private static double GetRate(string webData, string key)
        {
            var pos = webData.IndexOf(key, StringComparison.Ordinal); // currency code 
            if (pos == -1) return -1;
            pos = webData.IndexOf("<td", pos + 1, StringComparison.Ordinal); // за сколько единиц
            pos = webData.IndexOf("<td", pos + 1, StringComparison.Ordinal); // русское название
            var startRatePos = webData.IndexOf("<td", pos + 1, StringComparison.Ordinal) + 4; // currency rate
            var endRatePos = webData.IndexOf("</td>", startRatePos, StringComparison.Ordinal);
            var rateString = webData.Substring(startRatePos, endRatePos - startRatePos).Trim();
            var rateClearString = new StringBuilder();
            foreach (var symbol in rateString)
            {
                if (char.IsNumber(symbol)) rateClearString.Append(symbol);
                else if (symbol == ',')
//                    rateClearString.Append(CultureInfo.InvariantCulture.NumberFormat.CurrencyDecimalSeparator);
                    rateClearString.Append('.'); // i prefer numbers in "en-US" cultureInfo
            }
            double rate;
            return double.TryParse(rateClearString.ToString(), NumberStyles.Any, new CultureInfo("en-US"), out rate) ? rate : -1;
        }

    }
}
