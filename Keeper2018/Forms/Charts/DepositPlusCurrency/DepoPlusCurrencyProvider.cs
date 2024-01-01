using System;
using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public class DepoPlusCurrencyProvider
    {
        private readonly KeeperDataModel _dataModel;

        public DepoPlusCurrencyProvider(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public IEnumerable<DepoCurrencyData> Evaluate()
        {
            var start = new DateTime(2002, 1, 1);
            while (start < DateTime.Today)
            {
                var mam = Analyze(start);
                yield return new DepoCurrencyData()
                {
                    StartDate = start,
                    DepoRevenue = mam.Item1,
                    CurrencyRatesDifferrence = mam.Item2,
                };
                start = start.AddMonths(1);
            }
        }

        private Tuple<decimal, decimal> Analyze(DateTime startMoment)
        {
            var trafficCalculator = new TrafficOfBranchCalculator(_dataModel, _dataModel.MineRoot(),
                new Period(new DateTime(2001, 12, 31), startMoment.Date.AddSeconds(-1)));
            trafficCalculator.EvaluateAccount();
            var before = trafficCalculator.TotalAmount;

            trafficCalculator = new TrafficOfBranchCalculator(_dataModel, _dataModel.MineRoot(),
                new Period(new DateTime(2001, 12, 31), startMoment.GetEndOfMonth()));
            trafficCalculator.EvaluateAccount();
            var after = trafficCalculator.TotalAmount;

            var result = _dataModel.SortMonthIncome(startMoment, startMoment.GetEndOfMonth());
            var depoTotal = result.BranchTotal(IncomeCategories.депозиты);

            var total = result.Dict.Sum(v=>v.Value.Sum(c=>c.Item2));

            var expense = _dataModel
                .CollectExpenseList(startMoment, startMoment.GetEndOfMonth(), false, total);

            return new Tuple<decimal, decimal>(depoTotal, after - (before + total - expense.Item2) );
        }
    }
}