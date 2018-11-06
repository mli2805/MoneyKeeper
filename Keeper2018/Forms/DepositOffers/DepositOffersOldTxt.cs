using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keeper2018
{
    public class DepositOffersOldTxt
    {
        public static async Task<List<DepositOffer>> LoadFromOldTxtAsync(List<Account> accountsPlaneList)
        {
            await Task.Delay(1);
            return LoadFromOldTxt(accountsPlaneList).ToList();
        }

        private static IEnumerable<DepositOffer> LoadFromOldTxt(List<Account> accountsPlaneList)
        {
            var content = File.ReadAllLines(DbUtils.GetTxtFullPath("BankDepositOffers.txt"), Encoding.GetEncoding("Windows-1251")).
                Where(s => !String.IsNullOrWhiteSpace(s)).ToList();

            foreach (var line in content)
            {
                var oneDepositOffer = DepositOfferFromString(line, accountsPlaneList);
               

                yield return (oneDepositOffer);
            }
        }

        private static DepositOffer DepositOfferFromString(string s, List<Account> accountsPlaneList)
        {
            var substrings = s.Split(';');
            return new DepositOffer()
            {
                Id = Convert.ToInt32(substrings[0]),
                Bank = accountsPlaneList.First(account => account.Name == substrings[1].Trim()),
                Title = substrings[2].Trim(),
                Essentials = new Dictionary<DateTime, DepositEssential>(),
                Comment = substrings[6].Trim()
            };
        }

        public DepositCalculationRules DepositOfferRulesFromString(string str)
        {
            var rules = new DepositCalculationRules();
            var array = str.Split(';');
            var s = array[0].Trim();

            rules.IsFactDays = s[0] == '1';
            rules.EveryStartDay = s[1] == '1';
            rules.EveryFirstDayOfMonth = s[2] == '1';
            rules.EveryLastDayOfMonth = s[3] == '1';
            rules.IsCapitalized = s[4] == '1';
            rules.IsRateFixed = s[5] == '1';
            rules.HasAdditionalProcent = s[6] == '1';

            rules.AdditionalProcent = Decimal.Parse(array[1]);
            return rules;
        }

        public void DepositRateLineFromString(string s, IEnumerable<DepositOffer> depositOffers)
        {
            var depositRateLine = new DepositRateLine();
            var substrings = s.Split(';');
            var depositOffer = depositOffers.First(offer => offer.Id == Convert.ToInt32(substrings[0]));
            if (depositOffer.Essentials.First().Value.RateLines == null) depositOffer.Essentials.First().Value.RateLines = new List<DepositRateLine>();
            depositRateLine.DateFrom = Convert.ToDateTime(substrings[1], new CultureInfo("ru-RU"));
            depositRateLine.AmountFrom = Convert.ToDecimal(substrings[2], new CultureInfo("en-US"));
            depositRateLine.AmountTo = Convert.ToDecimal(substrings[3], new CultureInfo("en-US"));
            depositRateLine.Rate = Convert.ToDecimal(substrings[4], new CultureInfo("en-US"));

            depositOffer.Essentials.First().Value.RateLines.Add(depositRateLine);
        }
    }
}
