using System;
using System.Composition;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Keeper.DomainModel.WorkTypes;
using Keeper.Models;
using Keeper.Utils.MonthAnalysis;

namespace Keeper.ViewModels.SingleViews
{
    [Export]
    class MonthAnalysisViewModel : Screen
    {
        private readonly MonthAnalyzer _monthAnalyzer;
        private readonly MonthAnalysisBlankInscriber _inscriber;

        private bool _isMonthEnded;

        private DateTime _startDate;
        public DateTime StartDate
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                _isMonthEnded = DateTime.Today.Year > _startDate.Date.Year ||
                   (DateTime.Today.Year == _startDate.Date.Year && DateTime.Today.Month > _startDate.Date.Month);
                ForecastListVisibility = !_isMonthEnded ? Visibility.Visible : Visibility.Collapsed;
                MonthAnalisysViewCaption = String.Format("Идет анализ месяца [{0}]...", String.Format("{0:MMMM yyyy}", StartDate));
                if (DateTime.Today.Year == _startDate.Date.Year && DateTime.Today.Month == _startDate.Date.Month) MonthAnalisysViewCaption += " - текущий период!";
                NotifyOfPropertyChange(() => MonthAnalisysViewCaption);
                MonthSaldo = _monthAnalyzer.AnalizeMonth(StartDate);
                MonthAnalisysViewCaption = String.Format("[{0}]", String.Format("{0:MMMM yyyy}", StartDate));
                NotifyOfPropertyChange(() => MonthAnalisysViewCaption);
            }
        }

        private Saldo _monthSaldo;
        public Saldo MonthSaldo
        {
            get { return _monthSaldo; }
            set
            {
                _monthSaldo = value;
                ResultForeground = MonthSaldo.BeginBalance.Common.TotalInUsd > MonthSaldo.EndBalance.Common.TotalInUsd ? Brushes.Red : Brushes.Blue;
                DepositResultForeground = MonthSaldo.Incomes.OnDeposits.TotalInUsd + MonthSaldo.ExchangeDepositDifference < 0 ? Brushes.Red : Brushes.Blue;
                Blank = _inscriber.FillInBlank(MonthSaldo, _isMonthEnded);
            }
        }

        private MonthAnalysisBlank _blank;
        public MonthAnalysisBlank Blank
        {
            get { return _blank; }
            set
            {
                if (Equals(value, _blank)) return;
                _blank = value;
                NotifyOfPropertyChange(() => Blank);
            }
        }

        private Visibility _forecastListVisibility;
        public Visibility ForecastListVisibility
        {
            get { return _forecastListVisibility; }
            set
            {
                if (Equals(value, _forecastListVisibility)) return;
                _forecastListVisibility = value;
                NotifyOfPropertyChange(() => ForecastListVisibility);
            }
        }

        private Brush _resultForeground;
        private Brush _depositResultForeground;

        public Brush ResultForeground
        {
            get { return _resultForeground; }
            set
            {
                if (Equals(value, _resultForeground)) return;
                _resultForeground = value;
                NotifyOfPropertyChange(() => ResultForeground);
            }
        }

        public Brush DepositResultForeground
        {
            get { return _depositResultForeground; }
            set
            {
                if (Equals(value, _depositResultForeground)) return;
                _depositResultForeground = value;
                NotifyOfPropertyChange(() => DepositResultForeground);
            }
        }

        public string MonthAnalisysViewCaption { get; set; }

        [ImportingConstructor]
        public MonthAnalysisViewModel(MonthAnalyzer monthAnalyzer, MonthAnalysisBlankInscriber inscriber)
        {
            _monthAnalyzer = monthAnalyzer;
            _inscriber = inscriber;
            StartDate = DateTime.Today;
        }

        public void ShowPreviousMonth()
        {
            StartDate = MonthSaldo.StartDate.AddMonths(-1);
        }

        public void ShowNextMonth()
        {
            StartDate = MonthSaldo.StartDate.AddMonths(1);
        }

        public void ShowPreviousQuarter()
        {
            StartDate = MonthSaldo.StartDate.AddMonths(-3);
        }

        public void ShowNextQuarter()
        {
            StartDate = MonthSaldo.StartDate.AddMonths(3);
        }
        public void ShowNextYear()
        {
            StartDate = MonthSaldo.StartDate.AddYears(1);
        }

        public void ShowPreviousYear()
        {
            StartDate = MonthSaldo.StartDate.AddYears(-1);
        }

        public void CloseView()
        {
            TryClose();
        }
    }
}
