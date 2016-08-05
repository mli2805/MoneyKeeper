using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Extentions;
using Keeper.DomainModel.Trans;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.AccountEditing;
using Keeper.Utils.BalanceEvaluating.Ilya;
using Keeper.Utils.DiagramDomainModel;

namespace Keeper.Utils.Diagram
{
    [Export]
    public class DiagramDataExtractorFromDb
    {
        private readonly KeeperDb _db;
        private readonly AccountTreeStraightener _accountTreeStraightener;
        private readonly MoneyBagConvertor _moneyBagConvertor;
        private readonly CurrencyRatesAsDictionary _ratesAsDictionary;

        [ImportingConstructor]
        public DiagramDataExtractorFromDb(KeeperDb db, AccountTreeStraightener accountTreeStraightener, MoneyBagConvertor moneyBagConvertor)
        {
            _db = db;
            _accountTreeStraightener = accountTreeStraightener;
            _moneyBagConvertor = moneyBagConvertor;
            _ratesAsDictionary = new CurrencyRatesAsDictionary(_db.CurrencyRates.ToList());
        }

        #region Core calculations

        public Dictionary<DateTime, MoneyBag> DepositBalancesForPeriodInCurrencies(Period period)
        {
            var result = new Dictionary<DateTime, MoneyBag>();
            var currentMoneyBag = new MoneyBag();
            var currentDate = period.Start;

            foreach (var tran in _db.TransWithTags)
            {
                if (currentDate != tran.Timestamp.Date)
                {
                    result.Add(currentDate, currentMoneyBag);
                    currentDate = tran.Timestamp.Date;
                }

                currentMoneyBag = currentMoneyBag + ProcessOneTran(tran);
            }
            return result;
        }

        private MoneyBag ProcessOneTran(TranWithTags tran)
        {
            var result = new MoneyBag();
            if (tran.MyAccount.IsDeposit()) result = tran.MoneyBagForAccount(tran.MyAccount);
            if (tran.MySecondAccount != null && tran.MySecondAccount.IsDeposit()) result = result + tran.MoneyBagForAccount(tran.MySecondAccount);
            return result;
        }

        /// <summary>
        /// довольно медленно
        /// может _ratesAsDictionary позволит ускорить?
        /// </summary>
        /// <param name="frequency"></param>
        /// <returns></returns>
        public List<DiagramPoint> GetBalances(Every frequency)
        {
            var root = _accountTreeStraightener.Seek("Мои", _db.Accounts);
            var result = new List<DiagramPoint>();
            var currentDate = new DateTime(2001, 12, 31); // считать надо всегда с самого начала, иначе остаток неправильный будет
            var currentMoneyBag = new MoneyBag();

            foreach (var tran in _db.TransWithTags)
            {
                while (currentDate < tran.Timestamp.Date)
                {
                    if (FunctionsWithEvery.IsLastDayOf(currentDate, frequency))
                    {
                        var sum = _moneyBagConvertor.MoneyBagToUsd(currentMoneyBag, tran.Timestamp.Date);
                        if (sum != 0) // если вернулся 0 - это гэпы без курсов в начале времен
                            result.Add(new DiagramPoint(currentDate, (double) sum));
                        else
                        {
                            var lastSum = result.Last().CoorYdouble;
                            result.Add(new DiagramPoint(currentDate, lastSum));
                        }
                    }
                    currentDate = currentDate.AddDays(1);

                }
                currentMoneyBag = currentMoneyBag + tran.MoneyBagForAccount(root);
            }
            result.Add(new DiagramPoint(currentDate, (double)_moneyBagConvertor.MoneyBagToUsd(currentMoneyBag, currentDate)));
            return result;
        }
        #endregion

        public List<DiagramPoint> MonthlyResults()
        {
            var result = new List<DiagramPoint>();
            var balances = GetBalances(Every.Month);

            for (var i = 1; i < balances.Count; i++)
            {
                result.Add(new DiagramPoint(balances[i].CoorXdate, balances[i].CoorYdouble - balances[i - 1].CoorYdouble));
            }
            return result;
        }

    }
}
