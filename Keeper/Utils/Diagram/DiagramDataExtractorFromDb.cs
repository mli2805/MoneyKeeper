using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.ByFunctional.AccountEditing;
using Keeper.DomainModel;
using Keeper.Utils.Common;
using Keeper.Utils.Rates;

namespace Keeper.Utils.Diagram
{
    class DiagramDataExtractorFromDb
    {
        private readonly KeeperDb _db;
        private readonly AccountTreeStraightener _accountTreeStraightener;
        private readonly RateExtractor _rateExtractor;
        private readonly CurrencyRatesAsDictionary _ratesAsDictionary;

        [ImportingConstructor]
        public DiagramDataExtractorFromDb(KeeperDb db, AccountTreeStraightener accountTreeStraightener)
        {
            _db = db;
            _accountTreeStraightener = accountTreeStraightener;
            _rateExtractor = new RateExtractor(db);
            _ratesAsDictionary = new CurrencyRatesAsDictionary(_db.CurrencyRates.ToList());
        }

        #region Core calculations

        /// <summary>
        /// если хотя бы для одной из валют в данный день нет курса, возвращаем 0 как признак что, что-то не так
        /// </summary>
        /// <param name="amounts"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public decimal ConvertAllToUsd(Dictionary<CurrencyCodes, decimal> amounts, DateTime date)
        {
            decimal sum = 0;
            foreach (var amount in amounts)
            {
                if (amount.Value.Equals(0)) continue;
                if (amount.Key == CurrencyCodes.USD) sum += amount.Value;
                else
                {
                    double rate;

                    Dictionary<DateTime, double> oneCurrency = _ratesAsDictionary.Rates[amount.Key];

                    if (!oneCurrency.TryGetValue(date, out rate)) return 0;
                    sum += amount.Value / (decimal)rate;
                }
            }
            return sum;
        }
        public decimal ConvertAllCurrenciesToUsd(Dictionary<CurrencyCodes, decimal> amounts, DateTime date)
        {
            decimal inUsd = 0;
            foreach (var amount in amounts)
            {
                inUsd += _rateExtractor.GetUsdEquivalent(amount.Value, amount.Key, date);
            }
            return inUsd;
        }

        public Dictionary<DateTime, Dictionary<CurrencyCodes, decimal>>
          AccountBalancesForPeriodInCurrencies(Account balancedAccount, Period period, bool includeDaysWithoutChanges)
        {
            var result = new Dictionary<DateTime, Dictionary<CurrencyCodes, decimal>>();
            var balanceInCurrencies = new Dictionary<CurrencyCodes, decimal>();
            var currentDate = period.Start;

            foreach (var transaction in _db.Transactions)
            {
                if (currentDate != transaction.Timestamp.Date)
                {
                    result.Add(currentDate, new Dictionary<CurrencyCodes, decimal>(balanceInCurrencies));
                    currentDate = currentDate.AddDays(1);
                    while (currentDate < transaction.Timestamp.Date)
                    {
                        if (includeDaysWithoutChanges) result.Add(currentDate, balanceInCurrencies);
                        currentDate = currentDate.AddDays(1);
                    }
                }

                if (transaction.Debet.Is(balancedAccount))
                {
                    if (!balanceInCurrencies.ContainsKey(transaction.Currency))
                        balanceInCurrencies.Add(transaction.Currency, -transaction.Amount);
                    else balanceInCurrencies[transaction.Currency] -= transaction.Amount;
                }

                if (transaction.Credit.Is(balancedAccount))
                {
                    if (!balanceInCurrencies.ContainsKey(transaction.Currency)) balanceInCurrencies.Add(transaction.Currency, transaction.Amount);
                    else balanceInCurrencies[transaction.Currency] += transaction.Amount;
                }

            }
            return result;
        }

        private static void TakeAmountIfItsNecessary(Account balancedAccount, Transaction transaction,
                                                 ref Dictionary<CurrencyCodes, decimal> balanceInCurrencies)
        {
            if (transaction.Debet.Is(balancedAccount))
            {
                if (!balanceInCurrencies.ContainsKey(transaction.Currency))
                    balanceInCurrencies.Add(transaction.Currency, -transaction.Amount);
                else balanceInCurrencies[transaction.Currency] -= transaction.Amount;
            }

            if (transaction.Credit.Is(balancedAccount))
            {
                if (!balanceInCurrencies.ContainsKey(transaction.Currency))
                    balanceInCurrencies.Add(transaction.Currency, transaction.Amount);
                else balanceInCurrencies[transaction.Currency] += transaction.Amount;
            }
        }


        // получение остатка по счету за каждую дату периода 
        // реализовано не через функцию получения остатка на дату, вызванную для дат периода
        // а за один проход по БД с получением остатков накопительным итогом, т.к. гораздо быстрее
        // forht version
        // изменено извлечение курсов - fifth version
        public Dictionary<DateTime, decimal> AccountBalancesForPeriodInUsd(Account balancedAccount, Period period, Every frequency)
        {
            var result = new Dictionary<DateTime, decimal>();
            var balanceInCurrencies = new Dictionary<CurrencyCodes, decimal>();
            var currentDate = new DateTime(2001, 12, 31); // считать надо всегда с самого начала, иначе остаток неправильный будет

            foreach (var transaction in _db.Transactions)
            {
                while (currentDate < transaction.Timestamp.Date)
                {
                    if (FunctionsWithEvery.IsLastDayOf(currentDate, frequency))
                    {
                        var sum = ConvertAllToUsd(balanceInCurrencies, currentDate);
                        if (sum != 0) // если вернулся 0 - это гэпы без курсов в начале времен
                            result.Add(currentDate, sum);
                        else
                        {
                            var lastSum = result.Last().Value;
                            result.Add(currentDate,lastSum);
                        }
                    }
                    currentDate = currentDate.AddDays(1);

                }
                TakeAmountIfItsNecessary(balancedAccount, transaction, ref balanceInCurrencies);
            }
            result.Add(currentDate, ConvertAllToUsd(balanceInCurrencies, currentDate));
            return result;
        }

        public Dictionary<DateTime, decimal> TrafficForPeriodInUsdByKategories(Account kategory, Period period, Every frequency)
        {
            var myAccounts = _accountTreeStraightener.Seek("Мои", _db.Accounts);
            var result = (from t in _db.Transactions
                where period.ContainsAndTimeWasChecked(t.Timestamp) && t.Article != null && t.Article.Is(kategory)
                join r in _db.CurrencyRates on new {t.Timestamp.Date, t.Currency} equals
                    new {r.BankDay.Date, r.Currency} into g
                from rate in g.DefaultIfEmpty()
                select new
                {
                    LastDayForEvery = FunctionsWithEvery.GetLastDayOfTheSamePeriod(t.Timestamp.Date, frequency),
                    AmountInUsd =
                        rate != null
                            ? t.Amount/(decimal) rate.Rate*t.SignForAmount(myAccounts)
                            : t.Amount*t.SignForAmount(myAccounts)
                }).
                GroupBy(l => l.LastDayForEvery).
                Select(res => new
                {
                    date = res.First().LastDayForEvery,
                    amount = res.Sum(s => s.AmountInUsd)
                }).ToDictionary(d => d.date, d=> d.amount);
            return result;
        }
        #endregion

        // на сколько изменился остаток по счету за месяц (разница остатка после Nного и N-1го месяцев)
        // годится именно для счетов, (не для категорий, на которых нет остатка, есть движение)
        public Dictionary<DateTime, decimal> MonthlyResults(string accountName)
        {
            var result = new Dictionary<DateTime, decimal>();

            var accountForAnalisys = (from account in new AccountTreeStraightener().Flatten(_db.Accounts) where account.Name == accountName select account).FirstOrDefault();
            var balances = AccountBalancesForPeriodInUsd(accountForAnalisys,
                                                         new Period(new DateTime(2001, 12, 31), DateTime.Now),
                                                         Every.Month).OrderBy(pair => pair.Key).ToList();

            for (var i = 1; i < balances.Count; i++)
            {
                result.Add(balances[i].Key, balances[i].Value - balances[i - 1].Value);
            }
            return result;
        }

        // какие обороты были по счету за месяц
        // применяется для счетов - категорий

        public Dictionary<DateTime, decimal> MonthlyTraffic(string accountName)
        {
            var kategory = (from account in new AccountTreeStraightener().Flatten(_db.Accounts) where account.Name == accountName select account).FirstOrDefault();

            return TrafficForPeriodInUsdByKategories(kategory, new Period(new DateTime(2002, 1, 1), DateTime.Now), Every.Month);
        }

        private static int MonthCountFromStart(DateTime date)
        {
            return (date.Year - 2002) * 12 + date.Month;
        }

        public Dictionary<DateTime, decimal> Average12MsByDictionary(Dictionary<DateTime, decimal> originalDictionary)
        {
            var result = new Dictionary<DateTime, decimal>();
            var last12MonthsValues = new SortedDictionary<DateTime, decimal>();
            foreach (var originalPair in originalDictionary.OrderBy(pair => pair.Key))
            {
                if (last12MonthsValues.Count == 12)
                    last12MonthsValues.Remove(last12MonthsValues.Min(pair => pair.Key));

                last12MonthsValues.Add(originalPair.Key, originalPair.Value);
                var averageForLast12Months = last12MonthsValues.Sum(pair => pair.Value) / last12MonthsValues.Count;
                result.Add(originalPair.Key, averageForLast12Months);
            }
            return result;
        }

        public List<Dictionary<DateTime, decimal>> ThreeAverageByDictionary(Dictionary<DateTime, decimal> originalDictionary)
        {
            var averageFromStartDictionary = new Dictionary<DateTime, decimal>();
            var averageFromJanuaryDictionary = new Dictionary<DateTime, decimal>();
            var averageForLast12MonthsDictionary = new Dictionary<DateTime, decimal>();

            decimal averageFromStart = 0;
            decimal averageFromJanuary = 0;
            var last12Months = new SortedDictionary<DateTime, decimal>();
            foreach (var originalPair in originalDictionary.OrderBy(pair => pair.Key))
            {
                averageFromStart += originalPair.Value;
                averageFromStartDictionary.Add(originalPair.Key, Math.Round(averageFromStart / MonthCountFromStart(originalPair.Key)));

                if (originalPair.Key.Month == 1) averageFromJanuary = 0;
                averageFromJanuary += originalPair.Value;
                averageFromJanuaryDictionary.Add(originalPair.Key, Math.Round(averageFromJanuary / originalPair.Key.Month));

                if (last12Months.Count < 12) last12Months.Add(originalPair.Key, originalPair.Value);
                else
                {
                    var minDate = last12Months.Min(pair => pair.Key);
                    last12Months.Remove(minDate);
                    last12Months.Add(originalPair.Key, originalPair.Value);
                    var averageForLast12Months = last12Months.Sum(pair => pair.Value) / 12;
                    averageForLast12MonthsDictionary.Add(originalPair.Key, averageForLast12Months);
                }
            }

            return new List<Dictionary<DateTime, decimal>>
               {
                 averageFromStartDictionary,
                 averageFromJanuaryDictionary,
                 averageForLast12MonthsDictionary
               };
        }

    }
}
