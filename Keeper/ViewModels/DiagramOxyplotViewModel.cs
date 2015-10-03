using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils.Common;
using Keeper.Utils.Diagram;
using Keeper.Utils.OxyPlots;
using OxyPlot;
using OxyPlot.Series;
using System.Linq;

namespace Keeper.ViewModels
{
    [Export]
    internal class DiagramOxyplotViewModel : Screen
    {
        private Tuple<YearMonth, YearMonth> _selectedInterval;
        public Tuple<YearMonth, YearMonth> SelectedInterval
        {
            get { return _selectedInterval; }
            set
            {
                _selectedInterval = value;
                SelectedPeriodTitle = YearMonth.IntervalToString(value);
                InitializeDiagram();
            }
        }

        public double FromPoint
        {
            get { return _fromPoint; }
            set
            {
                if (value.Equals(_fromPoint)) return;
                _fromPoint = value;
                SelectedInterval = _boxBox.PointsToYearMonth(_fromPoint, _toPoint);
                NotifyOfPropertyChange();
            }
        }

        public double ToPoint
        {
            get { return _toPoint; }
            set
            {
                if (value.Equals(_toPoint)) return;
                _toPoint = value;
                SelectedInterval = _boxBox.PointsToYearMonth(_fromPoint, _toPoint);
                NotifyOfPropertyChange();
            }
        }

        public string SelectedPeriodTitle
        {
            get { return _selectedPeriodTitle; }
            set
            {
                if (value == _selectedPeriodTitle) return;
                _selectedPeriodTitle = value;
                NotifyOfPropertyChange();
            }
        }

        public DiagramIntervalMode IntervalMode { get; set; }

        private readonly List<ExpensePartingDataElement> _diagramData;
        private PlotModel _myPlotModel;
        private double _fromPoint;
        private double _toPoint;
        private string _selectedPeriodTitle;

        public ObservableCollection<string> LegendBindingSource
        {
            get { return _legendBindingSource; }
            set
            {
                if (Equals(value, _legendBindingSource)) return;
                _legendBindingSource = value;
                NotifyOfPropertyChange(() => LegendBindingSource);
            }
        }

        public PlotModel MyPlotModel
        {
            get { return _myPlotModel; }
            set
            {
                if (Equals(value, _myPlotModel)) return;
                _myPlotModel = value;
                NotifyOfPropertyChange(() => MyPlotModel);
            }
        }


        private BlackBox _boxBox;
        private ObservableCollection<string> _legendBindingSource;

        public DiagramOxyplotViewModel(List<ExpensePartingDataElement> diagramData)
        {
            _diagramData = diagramData;

            SelectedInterval = new Tuple<YearMonth, YearMonth>(new YearMonth(DateTime.Today.AddMonths(-24)),new YearMonth(DateTime.Today) );
            IntervalMode = DiagramIntervalMode.Months;

            // min / max is defined by time
            _boxBox = new BlackBox(_diagramData.Min().YearMonth, _diagramData.Max().YearMonth, IntervalMode);

            _boxBox.YearMonthPeriodToPoints(SelectedInterval, out _fromPoint, out _toPoint);

            InitializeDiagram();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Распределение расходов";
        }

        private void InitializeDiagram()
        {
            var temp = new PlotModel();
            var pieData = Extract(SelectedInterval).ToList();
            temp.Series.Add(InitializePieSeries(pieData));
            MyPlotModel = temp; // this is raising the INotifyPropertyChanged event			
            LegendBindingSource = InitializeLegend(pieData);
        }

        private Series InitializePieSeries(IEnumerable<ExpensePartingDataElement> pieData)
        {
            var series = new PieSeries();
            foreach (var element in pieData)
            {
                series.Slices.Add(new PieSlice(element.Kategory.Name, (double)element.Amount));
            }
            return series;
        }

        private ObservableCollection<string> InitializeLegend(List<ExpensePartingDataElement> pieData)
        {
            var result = new ObservableCollection<string>();
            var sum = pieData.Sum(a => a.Amount);
            foreach (var element in pieData)
            {
                result.Add(string.Format("{0} - {1}%  (${2:0,0})", element.Kategory, Math.Round(element.Amount/sum*100,0), Math.Round(element.Amount,0)));
            }
            result.Add(string.Format("Всего  ${0:0,0}", Math.Round(sum)));
            return result;
        }

        private IEnumerable<ExpensePartingDataElement> Extract(Tuple<YearMonth, YearMonth> period)
        {
            var r = _diagramData.Where(a => a.YearMonth.InInterval(period))
                   .GroupBy(e => e.Kategory)
                   .Select(g => new ExpensePartingDataElement(g.First().Kategory,g.Sum(p=>p.Amount),g.First().YearMonth)).Where(k => k.Amount > 0).OrderByDescending(o => o.Amount);
            return r;
        }

    }
}
