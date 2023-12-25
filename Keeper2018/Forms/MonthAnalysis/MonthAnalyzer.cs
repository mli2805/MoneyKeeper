using System;
using System.Windows;
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
            var caption = isYearAnalysisMode ? $"{startDate.Year} год" : startDate.ToString("MMMM yyyy");

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
                : isYearAnalysisMode ? startDate.GetEndOfYear() : startDate.GetEndOfMonth();
            _monthAnalysisModel.FinishMoment = finishMoment;

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
            var word = _isYearAnalysisMode ? "года" : "месяца";
            _monthAnalysisModel.BeforeViewModel.List.Add("Входящий остаток на начало " + word, FontWeights.Bold);
            _monthAnalysisModel.BeforeViewModel.List.AddList(trafficCalculator.ReportForMonthAnalysis());
            _monthAnalysisModel.Before = trafficCalculator.TotalAmount;
        }

        private void FillIncomeList(DateTime startDate, DateTime finishMoment)
        {
            var income = _dataModel.CollectIncomeList(startDate, finishMoment, _isYearAnalysisMode);

            _monthAnalysisModel.IncomeViewModel = new BorderedListViewModel(income.Item1);
            _monthAnalysisModel.Income = income.Item2;
            _monthAnalysisModel.SteadyIncome = income.Item4;
        }

        private void FillIncomeForecastList(DateTime fromDate, DateTime finishMoment)
        {
            var forecast = _dataModel.ForecastIncome(fromDate, finishMoment);

            _monthAnalysisModel.IncomeForecastList = forecast.Item1;
            _monthAnalysisModel.IncomeForecast = forecast.Item2;
        }

        private void FillExpenseList(DateTime startDate, DateTime finishMoment)
        {
            var expense = _dataModel
                .CollectExpenseList(startDate, finishMoment, _isYearAnalysisMode,
                  _monthAnalysisModel.Income, _monthAnalysisModel.SteadyIncome);

            _monthAnalysisModel.ExpenseViewModel = new BorderedListViewModel(expense.Item1);
            _monthAnalysisModel.Expense = expense.Item2;
            _monthAnalysisModel.LargeExpense = expense.Item3;
        }

        private void FillAfterList(DateTime finishMoment)
        {
            var trafficCalculator = new TrafficOfBranchCalculator(_dataModel, _dataModel.MineRoot(),
                                        new Period(new DateTime(2001, 12, 31), finishMoment));
            trafficCalculator.EvaluateAccount();
            var word = _isYearAnalysisMode ? "года" : "месяца";
            var text = _monthAnalysisModel.IsCurrentPeriod ? "сегодня" : "конец " + word;
            _monthAnalysisModel.AfterViewModel.List.Add($"Исходящий остаток на {text}", FontWeights.Bold);
            _monthAnalysisModel.AfterViewModel.List.AddList(trafficCalculator.ReportForMonthAnalysis());
            _monthAnalysisModel.After = trafficCalculator.TotalAmount;
        }
    }
}