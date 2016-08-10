using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.BalanceEvaluating;
using Keeper.Utils.Rates;

namespace Keeper.Utils.MonthAnalysis
{
    [Export]
    class MonthAnalyzer
    {
        public Saldo Result { get; set; }

        private readonly RateExtractor _rateExtractor;
        private readonly MonthTrafficAnalyzer _monthTrafficAnalyzer;
        private readonly MonthBalancesAnalyzer _monthBalancesAnalyzer;
        private readonly MonthForecaster _monthForecaster;

        [ImportingConstructor]
        public MonthAnalyzer(RateExtractor rateExtractor,
            MonthTrafficAnalyzer monthTrafficAnalyzer,
            MonthBalancesAnalyzer monthBalancesAnalyzer,
                             MonthForecaster monthForecaster
                             )
        {
            _rateExtractor = rateExtractor;
            _monthTrafficAnalyzer = monthTrafficAnalyzer;
            _monthBalancesAnalyzer = monthBalancesAnalyzer;
            _monthForecaster = monthForecaster;

            Result = new Saldo();
        }

        private List<CurrencyRate> InitializeRates(DateTime date)
        {
            var result = new List<CurrencyRate>();
            var currencyList = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>();
            foreach (CurrencyCodes currencyCode in currencyList)
            {
                if (currencyCode != CurrencyCodes.USD) result.Add(_rateExtractor.FindRateForDateOrBefore(currencyCode, date));
            }
            return result;
        }

        public Saldo AnalizeMonth(DateTime initialDay)
        {
            var sw = new Stopwatch();
            sw.Start();

            Result = new Saldo();
            Result.StartDate = initialDay.AddDays(-initialDay.Day + 1);
            Result.BeginBalance = _monthBalancesAnalyzer.GetExtendedBalanceBeforeDate(Result.StartDate);
            Result.BeginRates = InitializeRates(Result.StartDate.AddDays(-1));
            Result.Incomes = _monthTrafficAnalyzer.GetIncomes(initialDay);
            Result.Expense = _monthTrafficAnalyzer.GetExpense(initialDay);
            Result.DepoTraffic = _monthTrafficAnalyzer.GetDepoTraffic(initialDay, Result.Incomes.OnDeposits.TotalInUsd);
            Result.EndBalance = _monthBalancesAnalyzer.GetExtendedBalanceBeforeDate(Result.StartDate.AddMonths(1));
            Result.EndRates = InitializeRates(Result.StartDate.AddMonths(1));

            _monthForecaster.CollectEstimates(Result);

            sw.Stop();
            Console.WriteLine("Month analysis takes {0}", sw.Elapsed);

            return Result;
        }

    }
}
