using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KeeperDomain;

namespace Keeper2018
{
    public class BankOffersOldTxt
    {
        public static async Task<List<DepositOffer>> LoadFromOldTxtAsync()
        {
            await Task.Delay(1);
            return LoadFromOldTxt().ToList();
        }

        private static IEnumerable<DepositOffer> LoadFromOldTxt()
        {
            var content = File.ReadAllLines(DbIoUtils.GetOldTxtFullPath("BankDepositOffers.txt"), Encoding.GetEncoding("Windows-1251")).
                Where(s => !String.IsNullOrWhiteSpace(s)).ToList();

            var rateLines = LoadRatesFromOldTxt().ToList();
            foreach (var line in content)
            {
                var oneDepositOffer = DepositOfferFromString(line, rateLines);
                yield return (oneDepositOffer);
            }
        }

        private static IEnumerable<DepositRateLine> LoadRatesFromOldTxt()
        {
            var content = File.ReadAllLines(DbIoUtils.GetOldTxtFullPath("BankDepositOffersRates.txt"),
                Encoding.GetEncoding("Windows-1251")).ToList();
            foreach (var line in content)
            {
                var rateLine = DepositRateLineFromString(line);
                yield return rateLine;
            }
        }

        private static DepositOffer DepositOfferFromString(string s, List<DepositRateLine> rateLines)
        {
            var substrings = s.Split(';');
            var offer = new DepositOffer()
            {
                Id = Convert.ToInt32(substrings[0]),
               // Bank = accountsPlaneList.First(account => account.Name == substrings[1].Trim()).Id,
                Bank = int.Parse(substrings[1].Trim()),
                Title = substrings[2].Trim(),
                MainCurrency = (CurrencyCode) Enum.Parse(typeof (CurrencyCode), substrings[3].Trim()),
                Comment = substrings[6].Trim()
            };
            offer.Essentials = DepositOfferRulesFromString(s, offer.Id, rateLines);
            return offer;
        }

        private static Dictionary<DateTime, DepositEssential> DepositOfferRulesFromString(string str, int id, List<DepositRateLine> rateLines)
        {
            var essentials = new DepositEssential();
            var rules = new DepositCalculationRules();
            var array = str.Split(';');
            var s = array[4].Trim();

            rules.IsFactDays = s[0] == '1';
            rules.EveryStartDay = s[1] == '1';
            rules.EveryFirstDayOfMonth = s[2] == '1';
            rules.EveryLastDayOfMonth = s[3] == '1';
            rules.IsCapitalized = s[4] == '1';
            rules.IsRateFixed = s[5] == '1';
            rules.HasAdditionalProcent = s[6] == '1';

            rules.AdditionalProcent = double.Parse(array[5]);
            essentials.CalculationRules = rules;
            essentials.RateLines = rateLines.Where(l => l.DepositOfferId == id).ToList();

            return new Dictionary<DateTime, DepositEssential> {{essentials.RateLines.First().DateFrom, essentials}};
        }

        private static DepositRateLine DepositRateLineFromString(string s)
        {
            var substrings = s.Split(';');
            return new DepositRateLine
            {
                DepositOfferId = Convert.ToInt32(substrings[0], new CultureInfo("en-US")),
                DateFrom = Convert.ToDateTime(substrings[1], new CultureInfo("ru-RU")),
                AmountFrom = Convert.ToDecimal(substrings[2], new CultureInfo("en-US")),
                AmountTo = Convert.ToDecimal(substrings[3], new CultureInfo("en-US")),
                Rate = Convert.ToDecimal(substrings[4], new CultureInfo("en-US"))
            };
        }
    }
}
