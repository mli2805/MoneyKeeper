using System;
using System.Collections.Generic;
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

            var income = _dataModel
                .CollectIncomeList(startMoment, startMoment.GetEndOfMonth(), false);
            var expense = _dataModel
                .CollectExpenseList(startMoment, startMoment.GetEndOfMonth(), false, income.Item2, income.Item4);

            return new Tuple<decimal, decimal>(income.Item3, after - (before + income.Item2 - expense.Item2) );
        }
    }
}