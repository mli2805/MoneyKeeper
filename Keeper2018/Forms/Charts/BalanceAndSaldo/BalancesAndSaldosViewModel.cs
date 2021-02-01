using System.Windows;
using Caliburn.Micro;
using OxyPlot;
using OxyPlot.Axes;

#pragma warning disable 612

namespace Keeper2018
{
    public class BalancesAndSaldosViewModel : Screen
    {
        public PlotModel DailyBalancesModel { get; set; }

        private Visibility _dailyBalancesModelVisibility = Visibility.Visible;
        public Visibility DailyBalancesModelVisibility
        {
            get => _dailyBalancesModelVisibility;
            set
            {
                if (value == _dailyBalancesModelVisibility) return;
                _dailyBalancesModelVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public PlotModel MonthlySaldoModel { get; set; }

        private Visibility _monthlySaldoModelVisibility = Visibility.Collapsed;
        public Visibility MonthlySaldoModelVisibility
        {
            get => _monthlySaldoModelVisibility;
            set
            {
                if (value == _monthlySaldoModelVisibility) return;
                _monthlySaldoModelVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public PlotModel AnnualSaldoModel { get; set; }

        private Visibility _annualSaldoModelVisibility = Visibility.Collapsed;
        public Visibility AnnualSaldoModelVisibility
        {
            get => _annualSaldoModelVisibility;
            set
            {
                if (value == _annualSaldoModelVisibility) return;
                _annualSaldoModelVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "T - Toggle chart";
        }

        public void Initialize(KeeperDataModel dataModel)
        {
            var model = new BalanceAndSaldoModel();
            model.Initialize(dataModel);
            model.PrepareSeries();

            InitializeDailyBalances(model);
            InitializeMonthlyAndAnnualSaldo(model);
         }

        private void InitializeDailyBalances(BalanceAndSaldoModel model)
        {
            DailyBalancesModel = new PlotModel();

            DailyBalancesModel.Axes.Add(new DateTimeAxis()
            {
                Position = AxisPosition.Bottom,
                IntervalLength = 75,
                MinorIntervalType = DateTimeIntervalType.Days,
                IntervalType = DateTimeIntervalType.Days,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dash,
            });
            DailyBalancesModel.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Left, 
                MajorGridlineStyle = LineStyle.Automatic,
                MinorGridlineStyle = LineStyle.Automatic,
            });

            DailyBalancesModel.Series.Add(model.DailyBalancesSeries);
        }

        private void InitializeMonthlyAndAnnualSaldo(BalanceAndSaldoModel model)
        {
            MonthlySaldoModel = new PlotModel();
            AnnualSaldoModel = new PlotModel();
            MonthlySaldoModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Dash });
            AnnualSaldoModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Dash });

            MonthlySaldoModel.Series.Add(model.MonthlySaldoSeries);
            AnnualSaldoModel.Series.Add(model.AnnualSaldoSeries);

            MonthlySaldoModel.Axes.Add(new CategoryAxis(null, model.MonthLabels.ToArray()));
            AnnualSaldoModel.Axes.Add(new CategoryAxis(null, model.YearLabels.ToArray()));
        }

        private int _model = 1;
        public void ToggleModel()
        {
            if (_model == 1)
            {
                _model = 2;
                DailyBalancesModelVisibility = Visibility.Collapsed;
                MonthlySaldoModelVisibility = Visibility.Visible;
                AnnualSaldoModelVisibility = Visibility.Collapsed;
            }
            else if (_model == 2)
            {
                _model = 3;
                DailyBalancesModelVisibility = Visibility.Collapsed;
                MonthlySaldoModelVisibility = Visibility.Collapsed;
                AnnualSaldoModelVisibility = Visibility.Visible;
            }
            else
            {
                _model = 1;
                DailyBalancesModelVisibility = Visibility.Visible;
                MonthlySaldoModelVisibility = Visibility.Collapsed;
                AnnualSaldoModelVisibility = Visibility.Collapsed;
            }
        }
    }
}
