using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Keeper2018
{
    public class MonthAnalysisModel
    {
        public string MonthAnalysisViewCaption { get; set; }
        public DateTime StartDate { get; set; }
        public Visibility ForecastListVisibility { get; set; }

        public List<string> BeforeList { get; set; } = new List<string>();
        public decimal Before;
        public List<string> IncomeList { get; set; } = new List<string>();
        public decimal Income;
        public List<string> ExpenseList { get; set; } = new List<string>();
        public decimal Expense;
        public decimal LargeExpense;
        public List<string> AfterList { get; set; } = new List<string>();
        public decimal After;
        public List<string> ResultList { get; set; } = new List<string>();
        public string RatesChanges { get; set; }
        public List<string> ForecastList { get; set; } = new List<string>();

        public Brush ResultForeground { get; set; }
        public Brush DepositResultForeground { get; set; }

        public decimal ExchangeDifference => After - (Before + Income - Expense);
        public decimal FinResultWithDifference => Income - Expense + ExchangeDifference;

        public void FillResultList()
        {
            ResultList.Add("Финансовый результат");
            ResultList.Add("");
            ResultList.Add($"{Income:N} - {Expense:N} = {Income - Expense:N} usd");
            ResultList.Add("");
            ResultList.Add("Курсовые разницы");
            ResultList.Add("");
            ResultList.Add($"{After:N} - ({Before:N} + {Income:N} - {Expense:N})");
            ResultList.Add($"       = {ExchangeDifference:N} usd");
            ResultList.Add("");
            ResultList.Add("");
            ResultList.Add("С учетом курсовых разниц");
            ResultList.Add("");
            ResultList.Add($"{Income:N} - {Expense:N} + {ExchangeDifference:N} = {FinResultWithDifference:N} usd");

            ResultForeground = FinResultWithDifference > 0 ? Brushes.Blue : Brushes.Red;
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