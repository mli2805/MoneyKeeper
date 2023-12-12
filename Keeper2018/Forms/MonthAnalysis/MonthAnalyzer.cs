using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Keeper2018.BorderedList;
using KeeperDomain;

namespace Keeper2018
{
    public class MonthAnalyzer
    {
        private readonly KeeperDataModel _dataModel;
        private MonthAnalysisModel _monthAnalysisModel;

        private bool _isYearAnalysisMode;

        public MonthAnalyzer(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        private bool IsCurrentPeriod(DateTime startDate)
        {
            return _isYearAnalysisMode 
                ? DateTime.Today.Year == startDate.Date.Year 
                : DateTime.Today.Year == startDate.Date.Year &&
                  DateTime.Today.Month == startDate.Date.Month;
        }

        public MonthAnalysisModel AnalyzeFrom(DateTime startDate, bool isYearAnalysisMode = false)
        {
            _isYearAnalysisMode = isYearAnalysisMode;
            var isCurrentPeriod = IsCurrentPeriod(startDate);
            var caption = _isYearAnalysisMode ? $"{startDate.Year} год" : startDate.ToString("MMMM yyyy");

            _monthAnalysisModel = new MonthAnalysisModel()
            {
                IsYearAnalysisMode = isYearAnalysisMode,
                StartDate = startDate,
                MonthAnalysisViewCaption = caption + (isCurrentPeriod ? " - текущий период!" : ""),
                IsCurrentPeriod = isCurrentPeriod,
            };

            var startMoment = startDate.AddSeconds(-1);
            FillBeforeViewModel(startMoment);
            var finishMoment = isCurrentPeriod 
                ? DateTime.Today.GetEndOfDate() 
                : _isYearAnalysisMode ? startDate.GetEndOfYear() : startDate.GetEndOfMonth();

            FillIncomeList(startDate, finishMoment);
            if (isCurrentPeriod)
                // не важно режим года или месяца - доходы предвидятся только на текущий месяц
                FillIncomeForecastList(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1), finishMoment);
            FillExpenseList(startDate, finishMoment);
            FillAfterList(finishMoment);
            _monthAnalysisModel.RatesChanges = _dataModel.GetRatesDifference(startMoment, finishMoment);
            _monthAnalysisModel.FillResultList(isCurrentPeriod);
            if (isCurrentPeriod)
                _monthAnalysisModel.FillForecast(finishMoment, (decimal)_dataModel.GetRate(DateTime.Today, CurrencyCode.BYN).Value);

            return _monthAnalysisModel;
        }

        private void FillBeforeViewModel(DateTime startMoment)
        {
            var trafficCalculator = new TrafficOfBranchCalculator(_dataModel, _dataModel.MineRoot(),
                                        new Period(new DateTime(2001, 12, 31), startMoment));
            trafficCalculator.EvaluateAccount();
            _monthAnalysisModel.BeforeViewModel.List.Add("Входящий остаток на начало месяца", FontWeights.Bold);
            _monthAnalysisModel.BeforeViewModel.List.AddList(trafficCalculator.ReportForMonthAnalysis());
            _monthAnalysisModel.Before = trafficCalculator.TotalAmount;
        }

        private void FillIncomeList(DateTime startDate, DateTime finishMoment)
        {
            var income = _dataModel.CollectIncomeList(startDate, finishMoment, _isYearAnalysisMode);

            _monthAnalysisModel.IncomeViewModel = new BorderedListViewModel(income.Item1);
            _monthAnalysisModel.Income = income.Item2;
            _monthAnalysisModel.DepoIncome = income.Item3;
        }

        private void FillIncomeForecastList(DateTime fromDate, DateTime finishMoment)
        {
            var realIncomes = _dataModel.Transactions.Values.Where(t => t.Operation == OperationType.Доход
                                              && t.Timestamp >= fromDate && t.Timestamp <= finishMoment).ToList();
            var salaryAccountId = 204;
            var iitAccountId = 443;
            var optixsoftAccountId = 172;
            if (!realIncomes.Any(t => t.Tags.Select(tt => tt.Id).Contains(salaryAccountId)
                && t.Tags.Select(tt => tt.Id).Contains(iitAccountId)))
            {
                var salaryInUsdValue = 800;
                _monthAnalysisModel.IncomeForecastList.Add($"зарплата ИИТ {salaryInUsdValue} usd");
                _monthAnalysisModel.IncomeForecast += salaryInUsdValue;
            }
            if (!realIncomes.Any(t => t.Tags.Select(tt => tt.Id).Contains(salaryAccountId)
                           && t.Tags.Select(tt => tt.Id).Contains(optixsoftAccountId)))
            {
                var salaryInUsdValue = 1200;
                _monthAnalysisModel.IncomeForecastList.Add($"зарплата OptixSoft {salaryInUsdValue} usd");
                _monthAnalysisModel.IncomeForecast += salaryInUsdValue;
            }

            var depoMainFolder = _dataModel.AcMoDict[166];
            foreach (var depo in depoMainFolder.Children.Where(c => ((AccountItemModel)c).IsDeposit))
                ForeseeDepoIncome((AccountItemModel)depo);
        }

        private void ForeseeDepoIncome(AccountItemModel depo)
        {
            var depoMainCurrency = _dataModel.DepositOffers
                .First(o => o.Id == depo.BankAccount.DepositOfferId).MainCurrency;
            var currency = depoMainCurrency == CurrencyCode.BYR ? CurrencyCode.BYN : depoMainCurrency;

            var revenues = depo.GetRevenuesInThisMonth(_dataModel);
            foreach (var tuple in revenues)
            {
                _monthAnalysisModel.IncomeForecastList.
                    Add($"{depo.ShortName}  {tuple.Item2:#,0.00} {currency.ToString().ToLower()} {tuple.Item1:dd MMM}");
                _monthAnalysisModel.IncomeForecast += currency == CurrencyCode.USD
                    ? tuple.Item2
                    : _dataModel.AmountInUsd(DateTime.Today, depoMainCurrency, tuple.Item2);
            }

        }

        private void FillExpenseList(DateTime startDate, DateTime finishMoment)
        {
            var expense = _dataModel.CollectExpenseList(startDate, finishMoment, _isYearAnalysisMode);

            _monthAnalysisModel.ExpenseViewModel = new BorderedListViewModel(expense.Item1);
            _monthAnalysisModel.Expense = expense.Item2;
            _monthAnalysisModel.LargeExpense = expense.Item3;
        }

        private void FillAfterList(DateTime finishMoment)
        {
            var trafficCalculator = new TrafficOfBranchCalculator(_dataModel, _dataModel.MineRoot(),
                                        new Period(new DateTime(2001, 12, 31), finishMoment));
            trafficCalculator.EvaluateAccount();
            var text = _monthAnalysisModel.IsCurrentPeriod ? "сегодня" : "конец месяца";
            _monthAnalysisModel.AfterViewModel.List.Add($"Исходящий остаток на {text}", FontWeights.Bold);
            _monthAnalysisModel.AfterViewModel.List.AddList(trafficCalculator.ReportForMonthAnalysis());
            _monthAnalysisModel.After = trafficCalculator.TotalAmount;
        }
    }
}