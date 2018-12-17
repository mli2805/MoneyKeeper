using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Keeper2018.BorderedList;

namespace Keeper2018
{

    public class MonthAnalysisModel
    {
        public string MonthAnalysisViewCaption { get; set; }
        public DateTime StartDate { get; set; }
        public bool IsCurrentPeriod { get;set; }
        public Visibility ForecastListVisibility => IsCurrentPeriod ? Visibility.Visible : Visibility.Collapsed;

        public BorderedListViewModel BeforeViewModel { get; set; } = new BorderedListViewModel();
        public decimal Before;
        public BorderedListViewModel IncomeViewModel { get; set; } = new BorderedListViewModel();
        public decimal Income;
        public BorderedListViewModel ExpenseViewModel { get; set; } = new BorderedListViewModel();
        public decimal Expense;
        public decimal LargeExpense;
        public BorderedListViewModel AfterViewModel { get; set; } = new BorderedListViewModel();
        public decimal After;
        public BorderedListViewModel FinResultViewModel { get; set; } = new BorderedListViewModel();
        public string RatesChanges { get; set; }
        public List<string> ForecastList { get; set; } = new List<string>();

        public Brush DepositResultForeground { get; set; }


        public void FillResultList()
        {

            var profit = Income - Expense;
            var profitForeground = profit > 0 ? Brushes.Blue : Brushes.Red;
            FinResultViewModel.List.Add("Финансовый результат", profitForeground);
            FinResultViewModel.List.Add("");
            FinResultViewModel.List.Add($"{Income:N} - {Expense:N} = {profit:N} usd", profitForeground);
            FinResultViewModel.List.Add("");


            var exchangeDifference = After - (Before + Income - Expense);
            var exchangeForeground = exchangeDifference > 0 ? Brushes.Blue : Brushes.Red;
            FinResultViewModel.List.Add("Курсовые разницы", exchangeForeground);
            FinResultViewModel.List.Add("");
            FinResultViewModel.List.Add($"{After:N} - ({Before:N} + {Income:N} - {Expense:N})", exchangeForeground);
            FinResultViewModel.List.Add($"       = {exchangeDifference:N} usd", FontWeights.Bold, exchangeForeground);
            FinResultViewModel.List.Add("");
            FinResultViewModel.List.Add("");


            var finResultWithDifference = profit + exchangeDifference;
            var resultForeground = finResultWithDifference > 0 ? Brushes.Blue : Brushes.Red;
            FinResultViewModel.List.Add("С учетом курсовых разниц", resultForeground);
            FinResultViewModel.List.Add("");
            FinResultViewModel.List.Add($"{Income:N} - {Expense:N} + {exchangeDifference:N}", resultForeground);
            FinResultViewModel.List.Add($"        = {finResultWithDifference:N} usd", FontWeights.Bold, resultForeground);

        }

        public void FillForecast(DateTime finishMoment, decimal bynRate)
        {
            ForecastList.Add("Прогноз результата");
            ForecastList.Add("");
            ForecastList.Add("Прогноз доходов");
            ForecastList.Add("");
            ForecastList.Add("  Прогноз расходов");
            ForecastList.Add("");
            var dayExpense = (Expense - LargeExpense) / finishMoment.Day;
            ForecastList.Add("  Среднедневные расходы");
            ForecastList.Add($"    {dayExpense:#,0} usd ( {dayExpense * bynRate:#,0} byn )");
            var l = finishMoment.GetDaysInMonth();
            ForecastList.Add($"  За {l} дней составит ");
            ForecastList.Add($"    {dayExpense * l:#,0} usd ( {dayExpense * l * bynRate:#,0} byn )");
            if (LargeExpense > 0)
                ForecastList.Add($"   + крупные расходы {LargeExpense:#,0} usd");
            ForecastList.Add("");
            ForecastList.Add($"  Итого расходов {dayExpense * l + LargeExpense:#,0} usd");
        }
    }
}