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
        public List<string> IncomeList{ get; set; } = new List<string>();
        public List<string> ExpenseList{ get; set; } = new List<string>();
        public List<string> AfterList{ get; set; } = new List<string>();
        public List<string> ResultList{ get; set; } = new List<string>();
        public string RatesChanges { get; set; }
        public List<string> ForecastList { get; set; } = new List<string>();

        public Brush ResultForeground { get; set; }
        public Brush DepositResultForeground { get; set; }
    }
}