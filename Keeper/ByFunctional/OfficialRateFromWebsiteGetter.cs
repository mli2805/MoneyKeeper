using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Keeper.DomainModel;

namespace Keeper.ByFunctional
{
    [Export]
    public class OfficialRateFromWebsiteGetter
    {
        private decimal GetTodaysOfficialRate(CurrencyCodes currency)
        {
            var stream = GetNationalBankWebsiteMainPage();
            decimal result = 0;
            while (!stream.EndOfStream)
            {
                var oneString = stream.ReadLine();
                if (!oneString.Contains(GetCurrencyName(currency))) continue;

                var nextString = stream.ReadLine();
                result = ExtractRate(nextString);
                break;
            }
            stream.Close();
            return result;
        }

        private StreamReader GetNationalBankWebsiteMainPage()
        {
            const string url = "http://nbrb.by";
            var request = (HttpWebRequest)WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();
            return response.GetResponseStream() == null ? null : new StreamReader(response.GetResponseStream());
        }

        private string GetCurrencyName(CurrencyCodes currency)
        {
            switch (currency)
            {
                case CurrencyCodes.USD: return "Доллар США";
                case CurrencyCodes.EUR: return "Евро";
                case CurrencyCodes.RUB: return "Российский рубль";
                default : return "";
            }
        }

        private decimal ExtractRate(string str)
        {
            var separators = new []{'<','>'};
            var array = str.Split(separators);
            return Decimal.Parse(array[1]);
        }

    }
}
