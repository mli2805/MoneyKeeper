using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Keeper2018
{
    public static class DbClassesInstanceParser
    {
        public static CurrencyRates CurrencyRateFromString(this string s)
        {
            var rate = new CurrencyRates();
            var substrings = s.Split(';');
            rate.Date = Convert.ToDateTime(substrings[0], new CultureInfo("ru-RU"));
            rate.NbRates = NbRbRatesFromString(substrings[1]);
            rate.CbrRate.Usd = OneRateFromString(substrings[2]);
            rate.MyUsdRate = OneRateFromString(substrings[3]);
            rate.MyEurUsdRate = OneRateFromString(substrings[4]);
            return rate;
        }

        private static NbRbRates NbRbRatesFromString(string s)
        {
            var rate = new NbRbRates();
            var substrings = s.Split('|');
            rate.Usd = OneRateFromString(substrings[0]);
            rate.Euro = OneRateFromString(substrings[1]);
            rate.Rur = OneRateFromString(substrings[2]);
            return rate;
        }

        private static OneRate OneRateFromString(string s)
        {
            var substrings = s.Split('/');
            return new OneRate()
            {
                Value = double.Parse(substrings[0], new CultureInfo("en-US")),
                Unit = int.Parse(substrings[1]),
            };
        }

        public static Account AccountFromString(this string s)
        {
            var account = new Account();
            var substrings = s.Split(';');
            account.Id = Convert.ToInt32(substrings[0]);
            account.Name = substrings[1].Trim();
            account.OwnerId = Convert.ToInt32(substrings[2]);
            account.IsFolder = Convert.ToBoolean(substrings[3]);
            account.IsExpanded = Convert.ToBoolean(substrings[4]);
        //    account.Name = substrings[5].Trim();
            Console.WriteLine(substrings[5].Trim());
            return account;
        }

        public static Deposit DepositFromString(this string s)
        {
            var deposit = new Deposit();
            var substrings = s.Split(';');
            deposit.MyAccountId = Convert.ToInt32(substrings[0]);
            deposit.DepositOfferId = Convert.ToInt32(substrings[1]);
            deposit.Serial = substrings[2].Trim();
            deposit.StartDate = Convert.ToDateTime(substrings[3], new CultureInfo("ru-RU"));
            deposit.FinishDate = Convert.ToDateTime(substrings[4], new CultureInfo("ru-RU"));
            deposit.ShortName = substrings[5].Trim();
            deposit.Comment = substrings[6].Replace("|", "\r\n");
            return deposit;
        }

        public static PayCard CardFromString(this string s)
        {
            var card = new PayCard();
            var substrings = s.Split(';');
            card.MyAccountId = Convert.ToInt32(substrings[0]);
            card.CardNumber = substrings[1].Trim();
            card.CardHolder = substrings[2].Trim();
            card.PaymentSystem = (PaymentSystem)Enum.Parse(typeof(PaymentSystem), substrings[3]);
            card.IsPayPass = Convert.ToBoolean(substrings[4]);
            return card;
        }

        public static Transaction TransactionFromString(this string s)
        {
            var tran = new Transaction();
            var substrings = s.Split(';');
            tran.Timestamp = Convert.ToDateTime(substrings[0], new CultureInfo("ru-RU"));
            tran.Operation = (OperationType)Enum.Parse(typeof(OperationType), substrings[1]);
            tran.Receipt = int.Parse(substrings[2].Trim());

            tran.MyAccount = int.Parse(substrings[3].Trim());
            tran.MySecondAccount = int.Parse(substrings[4].Trim());

            tran.Amount = Convert.ToDecimal(substrings[5], new CultureInfo("en-US"));
            tran.Currency = (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[6]);
            tran.AmountInReturn = Convert.ToDecimal(substrings[7], new CultureInfo("en-US"));
            tran.CurrencyInReturn = substrings[8].Trim() != "" ? (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[8]) : CurrencyCode.USD;
            tran.Tags = TagsFromString(substrings[9].Trim());
            tran.Comment = substrings[10].Trim();

            return tran;
        }

        private static List<int> TagsFromString(string str)
        {
            var tags = new List<int>();
            if (str == "") return tags;

            var substrings = str.Split('|');
            tags.AddRange(substrings.Select(substring => int.Parse(substring.Trim())));

            return tags;
        }

        public static TagAssociation TagAssociationFromString(this string s)
        {
            var association = new TagAssociation();
            var substrings = s.Split(';');
            association.ExternalAccount = int.Parse(substrings[0].Trim());
            association.Tag = int.Parse(substrings[1].Trim());
            association.OperationType = (OperationType)Enum.Parse(typeof(OperationType), substrings[2]);
            association.Destination = (AssociationType)Enum.Parse(typeof(AssociationType), substrings[3]);
            return association;
        }

        public static DepositOffer DepositOfferFromString(this string s)
        {
            var substrings = s.Split(';');
            return new DepositOffer()
            {
                Id = Convert.ToInt32(substrings[0]),
                Bank = Convert.ToInt32(substrings[1]),
                Title = substrings[2].Trim(),
                MainCurrency = (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[3]),
                Comment = substrings[4].Trim()
            };
        }

        public static DepositEssential DepositEssentialFromString(this string str)
        {
            var substrings = str.Split(';');
            return new DepositEssential()
            {
                DepositOfferId = Convert.ToInt32(substrings[0]),
                Id = Convert.ToInt32(substrings[1]),
                CalculationRules = substrings[2].DepositOfferRulesFromString(),
                Comment = substrings[3].Trim(),
            };
        }

        public static DepositCalculationRules DepositOfferRulesFromString(this string str)
        {
            var rules = new DepositCalculationRules();
            var array = str.Split('+');
            var s = array[0].Trim();

            rules.IsFactDays = s[0] == '1';
            rules.EveryStartDay = s[1] == '1';
            rules.EveryFirstDayOfMonth = s[2] == '1';
            rules.EveryLastDayOfMonth = s[3] == '1';
            rules.IsCapitalized = s[4] == '1';
            rules.IsRateFixed = s[5] == '1';
            rules.HasAdditionalProcent = s[6] == '1';

            rules.AdditionalProcent = double.Parse(array[1]);
            return rules;
        }

        public static DepositRateLine DepositRateLineFromString(this string s)
        {
            var depositRateLine = new DepositRateLine();
            var substrings = s.Split(';');
            depositRateLine.DepositOfferId = int.Parse(substrings[0].Trim());
            depositRateLine.DepositOfferEssentialsId = int.Parse(substrings[1].Trim());
            depositRateLine.DateFrom = Convert.ToDateTime(substrings[2], new CultureInfo("ru-RU"));
            depositRateLine.AmountFrom = Convert.ToDecimal(substrings[3], new CultureInfo("en-US"));
            depositRateLine.AmountTo = Convert.ToDecimal(substrings[4], new CultureInfo("en-US"));
            depositRateLine.Rate = Convert.ToDecimal(substrings[5], new CultureInfo("en-US"));
            return depositRateLine;
        }
    }
}
