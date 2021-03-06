﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Extentions;
using Keeper.DomainModel.Trans;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.Rates;

namespace Keeper.Utils.BalancesFromTransWithTags
{
    [Export]
    public class BalancesForMainViewCalculator
    {
        private readonly KeeperDb _db;
        private readonly RateExtractor _rateExtractor;

        [ImportingConstructor]
        public BalancesForMainViewCalculator(KeeperDb db, RateExtractor rateExtractor)
        {
            _db = db;
            _rateExtractor = rateExtractor;
        }

        /// <summary>
        /// Единственная функция , которая торчит наружу
        /// на вход счет/тэг и дата/период 
        /// на выход остаток и строки , которые надо показать на главном экране
        /// </summary>
        /// <param name="selectedAccount"></param>
        /// <param name="period"></param>
        /// <param name="balanceList"></param>
        /// <returns></returns>
        public decimal FillListForShellView(Account selectedAccount, Period period, ObservableCollection<string> balanceList)
        {
            balanceList.Clear();
            if (selectedAccount == null) return 0;
            return selectedAccount.Children.Count == 0 ?
                FillListForAccountWithTraffic(selectedAccount, period, balanceList) :
                FillListForAccountWithChildren(selectedAccount, period, balanceList);
        }


        private MoneyBag GetMoneyBag(Account item, Period period)
        {
            return item.Is("Мои") ?
                _db.TransWithTags.Sum(t => t.MoneyBagForAccount(item, period)) :
                _db.TransWithTags.Sum(t => t.MoneyBagForTag(item, period));
        }
        private decimal FillListForAccountWithChildren(Account selectedAccount, Period period, ObservableCollection<string> balanceList)
        {
            MoneyBag total = new MoneyBag();
            var list = new List<string>();
            foreach (var child in selectedAccount.Children)
            {
                var childBag = GetMoneyBag(child, period);
                if (!childBag.IsZero())
                {
                    list.Add("      " + child.Name);
                    list.AddRange(MoneyBagToListOfStrings(childBag, "   "));
                    total += childBag;
                }
            }

            var mainList = MoneyBagToListOfStrings(total);
            mainList.AddRange(list);
            foreach (var str in mainList)
            {
                balanceList.Add(str);
            }
            return _rateExtractor.GetUsdEquivalent(total, period.Finish);
        }

        private static List<string> MoneyBagToListOfStrings(MoneyBag moneyBag, string gap = "")
        {
            var balanceList = new List<string>();
            var currencies = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();
            foreach (var currency in currencies)
            {
                var oneCurrencyBalance = moneyBag[currency];
                if (oneCurrencyBalance != 0)
                    balanceList.Add(currency == CurrencyCodes.BYR
                        ? gap + $"{oneCurrencyBalance:#,#} {currency}"
                        : gap + $"{oneCurrencyBalance:#,0.##} {currency}");
            }
            return balanceList;
        }

        private decimal FillListForAccountWithTraffic(Account selectedAccount, Period period, ObservableCollection<string> balanceList)
        {
            MoneyBag moneyBag = GetMoneyBag(selectedAccount, period);
            var mainList = MoneyBagToListOfStrings(moneyBag);

            foreach (var str in mainList)
            {
                balanceList.Add(str);
            }

            if (selectedAccount.Is("Мои"))
            {
                foreach (var str in GetTrafficListForAccount(selectedAccount, period))
                {
                    balanceList.Add("  " + str);
                }
                return _rateExtractor.GetUsdEquivalent(moneyBag, period.Finish);
            }
            else
            {
                var traffic = GetTrafficListForTag(selectedAccount, period);
                foreach (var tr in traffic)
                {
                    balanceList.Add("  " + tr);
                }
                return traffic.Sum(t => t.AmountInUsd);
            }
        }

        private List<TrafficOnMainPage> GetTrafficListForTag(Account tag, Period period)
        {
            return (from t in _db.TransWithTags
                    where period.ContainsAndTimeWasChecked(t.Timestamp) && t.Tags.Contains(tag)
                    join
                        r in _db.CurrencyRates
                        on new { t.Timestamp.Date, Currency = t.Currency.GetValueOrDefault() } equals
                        new { r.BankDay.Date, r.Currency } into g
                    from rate in g.DefaultIfEmpty()
                    select new TrafficOnMainPage()
                    {
                        Timestamp = t.Timestamp,
                        Amount = t.AmountForTag(tag, t.Currency),
                        Currency = t.Currency.GetValueOrDefault(),
                        Rate = rate?.Rate ?? 0,
                        Comment = t.Comment
                    }).OrderBy(t => t.Timestamp).ToList();
        }

        private List<TrafficOnMainPage> GetTrafficWhereMyAccountIsFirstOrOnly(Account account, Period period)
        {
            return (from
                t in _db.TransWithTags
                    where period.ContainsAndTimeWasChecked(t.Timestamp) && t.MyAccount.Is(account.Name)
                    join
                        r in _db.CurrencyRates
                        on new { t.Timestamp.Date, Currency = t.Currency.GetValueOrDefault() } equals
                        new { r.BankDay.Date, r.Currency } into g
                    from rate in g.DefaultIfEmpty()
                    select new TrafficOnMainPage()
                    {
                        Timestamp = t.Timestamp,
                        Amount = t.AmountForAccount(account, t.Currency),
                        Currency = t.Currency.GetValueOrDefault(),
                        Rate = rate?.Rate ?? 0,
                        Comment = t.Comment
                    }).ToList();
        }

        private IEnumerable<TrafficOnMainPage> GetTrafficWhereMyAccountIsSecond(Account account, Period period)
        {
            return from
                t in _db.TransWithTags
                   where period.ContainsAndTimeWasChecked(t.Timestamp) &&
                         t.MySecondAccount != null && t.MySecondAccount.Is(account.Name)
                   join
                       r in _db.CurrencyRates
                       on new { t.Timestamp.Date, Currency = t.CurrencyOfSecondAccount() } equals
                       new { r.BankDay.Date, r.Currency } into g
                   from rate in g.DefaultIfEmpty()
                   select new TrafficOnMainPage()
                   {
                       Timestamp = t.Timestamp,
                       Amount = t.AmountForAccount(account, t.CurrencyOfSecondAccount()),
                       Currency = t.CurrencyOfSecondAccount(),
                       Rate = rate?.Rate ?? 0,
                       Comment = t.Comment
                   };
        }

        private List<string> GetTrafficListForAccount(Account account, Period period)
        {
            var transWithRates1 = GetTrafficWhereMyAccountIsFirstOrOnly(account, period);
            var transWithRates2 = GetTrafficWhereMyAccountIsSecond(account, period);
            transWithRates1.AddRange(transWithRates2);
            return transWithRates1.OrderBy(t => t.Timestamp).Select(t => t.ToString()).ToList();
        }
    }
}


