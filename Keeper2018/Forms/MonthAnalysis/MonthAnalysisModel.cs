using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ObservableCollection<string> IncomeList{ get; set; } = new ObservableCollection<string>();
        public List<string> ExpenseList{ get; set; } = new List<string>();
        public List<string> AfterList{ get; set; } = new List<string>();
        public ObservableCollection<string> ResultList{ get; set; }
        public ObservableCollection<string> DepositResultList { get; set; }
        public ObservableCollection<string> RatesList { get; set; }
        public ObservableCollection<string> ForecastListIncomes { get; set; }
        public ObservableCollection<string> ForecastListExpense{ get; set; }
        public ObservableCollection<string> ForecastListBalance{ get; set; }

        public Brush ResultForeground { get; set; }
        public Brush DepositResultForeground { get; set; }
    }
}