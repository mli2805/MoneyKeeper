using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Keeper2018.BorderedList;

namespace Keeper2018
{
    public class MonthAnalysisModel
    {
        public string MonthAnalysisViewCaption { get; set; }
        public DateTime StartDate { get; set; }
        public bool IsCurrentPeriod { get; set; }
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
        public decimal ExchangeDifference;
        public BorderedListViewModel ForecastViewModel { get; set; } = new BorderedListViewModel();
        public decimal IncomeForecast;
        public decimal ExpenseForecast;
        public List<string> IncomeForecastList = new List<string>();

        public void FillResultList()
        {
            var profit = Income - Expense;
            var profitForeground = profit > 0 ? Brushes.Blue : Brushes.Red;
            FinResultViewModel.List.Add("Финансовый результат", FontWeights.Bold, profitForeground);
            FinResultViewModel.List.Add("");
            FinResultViewModel.List.Add($"{Income:N} - {Expense:N} = {profit:N} usd", profitForeground);
            FinResultViewModel.List.Add("");

            ExchangeDifference = After - (Before + Income - Expense);
            var exchangeForeground = ExchangeDifference > 0 ? Brushes.Blue : Brushes.Red;
            FinResultViewModel.List.Add("Курсовые разницы", exchangeForeground);
            FinResultViewModel.List.Add("");
            FinResultViewModel.List.Add($"{After:N} - ({Before:N} + {Income:N} - {Expense:N})", exchangeForeground);
            FinResultViewModel.List.Add($"       = {ExchangeDifference:N} usd", FontWeights.Bold, exchangeForeground);
            FinResultViewModel.List.Add("");
            FinResultViewModel.List.Add("");

            var finResultWithDifference = profit + ExchangeDifference;
            var resultForeground = finResultWithDifference > 0 ? Brushes.Blue : Brushes.Red;
            FinResultViewModel.List.Add("С учетом курсовых разниц", resultForeground);
            FinResultViewModel.List.Add("");
            FinResultViewModel.List.Add($"{Income:N} - {Expense:N} + {ExchangeDifference:N}", resultForeground);
            FinResultViewModel.List.Add($"        = {finResultWithDifference:N} usd", FontWeights.Bold, resultForeground);
        }

        public void FillForecast(DateTime finishMoment, decimal bynRate)
        {
            var dayExpense = (Expense - LargeExpense) / finishMoment.Day;
            var daysInMonth = finishMoment.GetDaysInMonth();
            ExpenseForecast = dayExpense * daysInMonth + LargeExpense;
            var result = Income + IncomeForecast - ExpenseForecast + ExchangeDifference;
            var resultBrush = result > 0 ? Brushes.Blue : Brushes.Red;
         
            ForecastViewModel.List.Add("Прогноз результата", FontWeights.Bold, resultBrush);
            ForecastViewModel.List.Add("");

            if (IncomeForecastList.Any())
            {
                ForecastViewModel.List.Add("  Еще ожидаются доходы");
                ForecastViewModel.List.Add("");
                foreach (var str in IncomeForecastList)
                    ForecastViewModel.List.Add("     " + str);
            }
            else
                ForecastViewModel.List.Add("  Все доходы учтены");
            ForecastViewModel.List.Add("");
            ForecastViewModel.List.Add($"  Итого доходов {Income:#,0} + {IncomeForecast:#,0} = {Income + IncomeForecast:#,0} usd", FontWeights.Bold);
            ForecastViewModel.List.Add("");

            ForecastViewModel.List.Add("  Среднедневные расходы");
            ForecastViewModel.List.Add($"    {dayExpense:#,0} usd ( {dayExpense * bynRate:#,0} byn )");
            ForecastViewModel.List.Add($"  За {daysInMonth} дней составит ");
            ForecastViewModel.List.Add($"    {dayExpense * daysInMonth:#,0} usd ( {dayExpense * daysInMonth * bynRate:#,0} byn )");
            if (LargeExpense > 0)
                ForecastViewModel.List.Add($"   + крупные расходы {LargeExpense:#,0} usd");
            ForecastViewModel.List.Add("");
            ForecastViewModel.List.Add($"  Итого расходов {ExpenseForecast:#,0} usd", FontWeights.Bold);

            ForecastViewModel.List.Add("");
            ForecastViewModel.List.Add("");
            ForecastViewModel.List.Add($"Итого {Income + IncomeForecast:#,0} - {ExpenseForecast:#,0} + {ExchangeDifference:#,0} = {result:#,0} usd",
                FontWeights.Bold, resultBrush);
         }
    }
}