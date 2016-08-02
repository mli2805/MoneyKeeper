using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Transactions;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.BalanceEvaluating.Ilya;
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
            return MoneyBagToUsd(total, period.Finish);
        }

        private decimal MoneyBagToUsd(MoneyBag moneyBag, DateTime date)
        {
            var currencies = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();
            decimal totalInUsd = 0;
            foreach (var currency in currencies)
            {
                totalInUsd += _rateExtractor.GetUsdEquivalent(moneyBag[currency], currency, date);
            }
            return totalInUsd;
        }

        private static List<string> MoneyBagToListOfStrings(MoneyBag result, string gap = "")
        {
            var balanceList = new List<string>();
            var currencies = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();
            foreach (var currency in currencies)
            {
                var oneCurrencyBalance = result[currency];
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
            }
            else
            {
                foreach (var str in GetTrafficListForTag(selectedAccount, period))
                {
                    balanceList.Add("  " + str);
                }

            }

            return MoneyBagToUsd(moneyBag, period.Finish);
        }

        private List<string> GetTrafficListForTag(Account tag, Period period)
        {
            var transWithRates = from t in _db.TransWithTags
                where period.ContainsAndTimeWasChecked(t.Timestamp) && t.Tags.Contains(tag)
                join
                    r in _db.CurrencyRates
                    on new {t.Timestamp.Date, Currency = t.Currency.GetValueOrDefault()} equals
                    new {r.BankDay.Date, r.Currency} into g
                from rate in g.DefaultIfEmpty()
                select new
                {
                    t.Timestamp,
                    Amount = t.AmountForTag(tag, t.Currency),
                    Currency = t.Currency.GetValueOrDefault(),
                    AmountInUsd =
                                          rate != null
                                              ? t.AmountForTag(tag, t.Currency) / (decimal)rate.Rate
                                              : t.AmountForTag(tag, t.Currency),
                    t.Comment
                };

            var list = new List<string>();
            foreach (var t in transWithRates.OrderBy(q => q.Timestamp))
            {
                switch (t.Currency)
                {
                    case CurrencyCodes.USD:
                        list.Add($"{t.Timestamp:d} {t.Amount} {t.Currency.ToString().ToLower()} {t.Comment}");
                        break;
                    case CurrencyCodes.BYR:
                        list.Add($"{t.Timestamp:d} {t.Amount:#,0} {t.Currency.ToString().ToLower()} ({t.AmountInUsd:#,0.00}$) {t.Comment}");
                        break;
                    default:
                        list.Add($"{t.Timestamp:d} {t.Amount:#,0.00} {t.Currency.ToString().ToLower()} ({t.AmountInUsd:#,0.00}$) {t.Comment}");
                        break;
                }

            }
            return list;

        }
        private List<string> GetTrafficListForAccount(Account account, Period period)
        {
            var transWithRates1 = (from
                                    t in _db.TransWithTags
                                  where period.ContainsAndTimeWasChecked(t.Timestamp) && t.MyAccount.Is(account.Name)
                                  join
                             r in _db.CurrencyRates
                             on new { t.Timestamp.Date, Currency = t.Currency.GetValueOrDefault() } equals
                                      new { r.BankDay.Date, r.Currency } into g
                                  from rate in g.DefaultIfEmpty()
                                  select new
                                  {
                                      t.Timestamp,
                                      Amount =  t.AmountForAccount(account, t.Currency),
                                      Currency = t.Currency.GetValueOrDefault(),
                                      AmountInUsd =
                                          rate != null
                                              ? t.AmountForAccount(account, t.Currency) / (decimal)rate.Rate
                                              : t.AmountForAccount(account, t.Currency),
                                      t.Comment
                                  }).ToList();

            var transWithRates2 = (from
                                    t in _db.TransWithTags
                                  where period.ContainsAndTimeWasChecked(t.Timestamp) &&
                                           t.MySecondAccount != null && t.MySecondAccount.Is(account.Name)
                                  join
                             r in _db.CurrencyRates
                             on new { t.Timestamp.Date, Currency = t.CurrencyInReturn == null ? t.Currency.GetValueOrDefault() : t.CurrencyInReturn.GetValueOrDefault()} equals
                                      new { r.BankDay.Date, r.Currency } into g
                                  from rate in g.DefaultIfEmpty()
                                  select new
                                  {
                                      t.Timestamp,
                                      Amount = t.AmountForAccount(account, t.CurrencyInReturn == null ? t.Currency.GetValueOrDefault() : t.CurrencyInReturn.GetValueOrDefault()),
                                      Currency = t.CurrencyInReturn == null ? t.Currency.GetValueOrDefault() : t.CurrencyInReturn.GetValueOrDefault(),
                                      AmountInUsd =
                                          rate != null
                                              ? t.AmountForAccount(account, t.CurrencyInReturn == null ? t.Currency.GetValueOrDefault() : t.CurrencyInReturn.GetValueOrDefault()) / (decimal)rate.Rate
                                              : t.AmountForAccount(account, t.CurrencyInReturn == null ? t.Currency.GetValueOrDefault() : t.CurrencyInReturn.GetValueOrDefault()),
                                      t.Comment
                                  });

            transWithRates1.AddRange(transWithRates2);

            var list = new List<string>();
            foreach (var t in transWithRates1.OrderBy(q => q.Timestamp))
            {
                switch (t.Currency)
                {
                    case CurrencyCodes.USD:
                        list.Add($"{t.Timestamp:d} {t.Amount} {t.Currency.ToString().ToLower()} {t.Comment}");
                        break;
                    case CurrencyCodes.BYR:
                        list.Add($"{t.Timestamp:d} {t.Amount:#,0} {t.Currency.ToString().ToLower()} ({t.AmountInUsd:#,0.00}$) {t.Comment}");
                        break;
                    default:
                        list.Add($"{t.Timestamp:d} {t.Amount:#,0.00} {t.Currency.ToString().ToLower()} ({t.AmountInUsd:#,0.00}$) {t.Comment}");
                        break;
                }

            }
            return list;
        }

    }
}
