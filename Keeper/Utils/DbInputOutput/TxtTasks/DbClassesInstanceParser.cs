using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Deposit;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Transactions;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
    [System.Composition.Export]
    public class DbClassesInstanceParser
    {

        public BankDepositOffer BankDepositOfferFromString(string s, IEnumerable<Account> accountsPlaneList)
        {
            var substrings = s.Split(';');
            return new BankDepositOffer(Convert.ToInt32(substrings[0]))
            {
                BankAccount = accountsPlaneList.First(account => account.Name == substrings[1].Trim()),
                DepositTitle = substrings[2].Trim(),
                Currency = (CurrencyCodes) Enum.Parse(typeof (CurrencyCodes), substrings[3]),
                CalculatingRules = DepositOfferRulesFromString(substrings[4] + " ; " + substrings[5]),
                Comment = substrings[6].Trim()
            };
        }

        public Transaction TransactionFromStringWithNames(string s, IEnumerable<Account> accountsPlaneList)
        {
            var transaction = new Transaction();
            var substrings = s.Split(';');
            transaction.Timestamp = Convert.ToDateTime(substrings[0], new CultureInfo("ru-RU"));
            transaction.Operation = (OperationType)Enum.Parse(typeof(OperationType), substrings[1]);
            transaction.Debet = accountsPlaneList.First(account => account.Name == substrings[2].Trim());
            transaction.Credit = accountsPlaneList.First(account => account.Name == substrings[3].Trim());
            transaction.Amount = Convert.ToDecimal(substrings[4], new CultureInfo("en-US"));
            transaction.Currency = (CurrencyCodes)Enum.Parse(typeof(CurrencyCodes), substrings[5]);
            transaction.Article = substrings[6].Trim() != "" ? accountsPlaneList.First(account => account.Name == substrings[6].Trim()) : null;
            transaction.Comment = substrings[7].Trim();
            transaction.Guid = new Guid(substrings[8].Trim());

            return transaction;
        }

        public TranWithTags TranWithTagsFromString(string s, IEnumerable<Account> accountsPlaneList)
        {
            var tran = new TranWithTags();
            var substrings = s.Split(';');
            tran.Timestamp = Convert.ToDateTime(substrings[0], new CultureInfo("ru-RU"));
            tran.Operation = (OperationType)Enum.Parse(typeof(OperationType), substrings[1]);
            tran.MyAccount = accountsPlaneList.First(account => account.Name == substrings[2].Trim());
            tran.MySecondAccount = substrings[3].Trim() != "" ? accountsPlaneList.First(account => account.Name == substrings[3].Trim()) : null;
            tran.Amount = Convert.ToDecimal(substrings[4], new CultureInfo("en-US"));
            tran.Currency = (CurrencyCodes)Enum.Parse(typeof(CurrencyCodes), substrings[5]);
            tran.AmountInReturn = Convert.ToDecimal(substrings[6], new CultureInfo("en-US"));
            tran.CurrencyInReturn = substrings[7].Trim() != "" ? (CurrencyCodes)Enum.Parse(typeof(CurrencyCodes), substrings[7]) : CurrencyCodes.USD;
            tran.Tags = TagsFromString(substrings[8].Trim(), accountsPlaneList); 
            tran.Comment = substrings[9].Trim();

            return tran;
        }

        private List<Account> TagsFromString(string str, IEnumerable<Account> accountsPlaneList)
        {
            var tags = new List<Account>();
            if (str == "") return tags;

            var substrings = str.Split('|');
            foreach (var substring in substrings)
            {
                tags.Add(accountsPlaneList.First(account => account.Name == substring.Trim()));
            }

            return tags;
        }
        public CurrencyRate CurrencyRateFromString(string s, IEnumerable<Account> accountsPlaneList)
        {
            var rate = new CurrencyRate();
            var next = s.IndexOf(';');
            rate.BankDay = Convert.ToDateTime(s.Substring(0, next), new CultureInfo("ru-RU"));
            rate.Currency = (CurrencyCodes)Enum.Parse(typeof(CurrencyCodes), s.Substring(next + 2, 3));
            next += 6;
            rate.Rate = Convert.ToDouble(s.Substring(next + 2), new CultureInfo("en-US"));
            return rate;
        }

        public NbRate OfficialRateFromString(string s, IEnumerable<Account> accountsPlaneList)
        {
            var rate = new NbRate();
            var substrings = s.Split(';');
            rate.Date = Convert.ToDateTime(substrings[0], new CultureInfo("ru-RU"));
            rate.UsdRate = Convert.ToDouble(substrings[1], new CultureInfo("en-US"));
            rate.EurRate = Convert.ToDouble(substrings[2], new CultureInfo("en-US"));
            rate.RurRate = Convert.ToDouble(substrings[3], new CultureInfo("en-US"));
            return rate;
        }
        public ArticleAssociation ArticleAssociationFromStringWithNames(string s, IEnumerable<Account> accountsPlaneList)
        {
            var association = new ArticleAssociation();
            var substrings = s.Split(';');
            association.ExternalAccount = accountsPlaneList.First(account => account.Name == substrings[0].Trim());
            association.OperationType = (OperationType)Enum.Parse(typeof(OperationType), substrings[1]);
            association.AssociatedArticle = accountsPlaneList.First(account => account.Name == substrings[2].Trim());
            return association;
        }
        public Account AccountFromString(string s, out int parentId)
        {
            var account = new Account();
            var substrings = s.Split(';');
            account.Id = Convert.ToInt32(substrings[0]);
            account.Name = substrings[1].Trim(); 
            parentId = Convert.ToInt32(substrings[2]);
            account.IsFolder = Convert.ToBoolean(substrings[3]);
            account.IsClosed = Convert.ToBoolean(substrings[4]);
            account.IsExpanded = Convert.ToBoolean(substrings[5]);
            return account;
        }
        public BankDepositCalculatingRules DepositOfferRulesFromString(string str)
        {
            var rules = new BankDepositCalculatingRules();
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
        public void DepositFromString(string s, IEnumerable<Account> accountsPlaneList, IEnumerable<BankDepositOffer> depositOffers)
        {
            var deposit = new Deposit();
            var substrings = s.Split(';');
            deposit.ParentAccount = accountsPlaneList.First(account => account.Id == Convert.ToInt32(substrings[0]));
            deposit.DepositOffer = depositOffers.First(offer => offer.Id == Convert.ToInt32(substrings[1]));
            deposit.AgreementNumber = substrings[2].Trim();
            deposit.StartDate = Convert.ToDateTime(substrings[3], new CultureInfo("ru-RU"));
            deposit.FinishDate = Convert.ToDateTime(substrings[4], new CultureInfo("ru-RU"));
            deposit.ShortName = substrings[5].Trim();
            deposit.Comment = substrings[6].Replace("|", "\r\n");

            deposit.ParentAccount.Deposit = deposit;
        }

        public void DepositRateLineFromString(string s, IEnumerable<BankDepositOffer> depositOffers)
        {
            var depositRateLine = new BankDepositRateLine();
            var substrings = s.Split(';');
            var depositOffer = depositOffers.First(offer => offer.Id == Convert.ToInt32(substrings[0]));
            if (depositOffer.RateLines == null) depositOffer.RateLines = new ObservableCollection<BankDepositRateLine>();
            depositRateLine.DateFrom = Convert.ToDateTime(substrings[1], new CultureInfo("ru-RU"));
            depositRateLine.AmountFrom = Convert.ToDecimal(substrings[2], new CultureInfo("en-US"));
            depositRateLine.AmountTo = Convert.ToDecimal(substrings[3], new CultureInfo("en-US"));
            depositRateLine.Rate = Convert.ToDecimal(substrings[4], new CultureInfo("en-US"));

            depositOffer.RateLines.Add(depositRateLine);
        }
    }
}
