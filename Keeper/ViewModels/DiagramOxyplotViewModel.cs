using System;
using System.Collections.Generic;
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
        public Period SelectedInterval
        {
            get { return _selectedInterval; }
            set
            {
                _selectedInterval = value;
                SelectedPeriodTitle = value.ToShortDateString();
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
                SelectedInterval = new Period(_boxBox.PointsToDates(_fromPoint, _toPoint));
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
                SelectedInterval = new Period(_boxBox.PointsToDates(_fromPoint, _toPoint));
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
        private Period _selectedInterval;
        private double _fromPoint;
        private double _toPoint;
        private string _selectedPeriodTitle;

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
        public DiagramOxyplotViewModel(List<ExpensePartingDataElement> diagramData)
        {
            _diagramData = diagramData;

            var today = DateTime.Today.GetEndOfDate();
            SelectedInterval = new Period(today.AddMonths(-1), today);
            IntervalMode = DiagramIntervalMode.Months;

            // min / max is defined by time
            _boxBox = new BlackBox(new DateTime(_diagramData.Min().Year, _diagramData.Min().Month, 15),
                new DateTime(_diagramData.Max().Year, _diagramData.Max().Month, 15),
                IntervalMode);
            var points = _boxBox.PeriodToPoints(SelectedInterval);
            FromPoint = points.Item1;
            ToPoint = points.Item2;

            InitializeDiagram();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Распределение расходов";
        }

        private void InitializeDiagram()
        {
            var temp = new PlotModel();
            temp.Series.Add(InitializePieSeries(Extract(SelectedInterval)));
            MyPlotModel = temp; // this is raising the INotifyPropertyChanged event			
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

        private IEnumerable<ExpensePartingDataElement> Extract(Period period)
        {
            return _diagramData.Where(a => period.ContainsAndTimeWasChecked(new DateTime(a.Year, a.Month, 15)));
        }

        public void PreviousPeriod()
        {
            var copy = (Period)SelectedInterval.Clone();
            if (IntervalMode == DiagramIntervalMode.Months) copy.MonthBack();
            else copy.YearBack();
            SelectedInterval = copy;
        }

        public void NextPeriod()
        {
            if (IntervalMode == DiagramIntervalMode.Months) SelectedInterval.MonthForward();
            else SelectedInterval.YearForward();
        }

    }
}
