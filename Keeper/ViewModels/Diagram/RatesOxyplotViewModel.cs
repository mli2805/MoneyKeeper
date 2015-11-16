using System.Collections.Generic;
using Caliburn.Micro;
using Keeper.Utils.DiagramDomainModel;
using Keeper.Utils.OxyPlots;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Keeper.ViewModels.Diagram
{
    class RatesOxyplotViewModel : Screen
    {
        private readonly DiagramData _diagramData;
        private PlotModel _myPlotModel;
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
        public RatesDiagramContentModel ContentModel { get; set; }

        public List<string> HintsSource { get; set; }
        public RatesOxyplotViewModel(DiagramData diagramData)
        {
            _diagramData = diagramData;
            ContentModel = new RatesDiagramContentModel()
            {
                IsCheckedUsdNbRb = true,
                IsCheckedMyUsd = true,
                IsCheckedEurNbRb = true,
                IsCheckedMyEur = true,
                IsCheckedRurNbRb = true,
                IsCheckedMyRur = true,
                IsCheckedBusketNbRb = true,
                IsCheckedLogarithm = false,
                IsCheckedUnify = false
            };
            InitializeDiagram();
        }

        private void InitializeDiagram()
        {
            HintsSource = new List<string>(){"Mouse wheel - Zoom", "Right button - Move", "Ctrl + Right button - Select rect"};

            var temp = new PlotModel();
            temp.Axes.Add(DefineDateTimeAxis());
            temp.Axes.Add(DefineRateAxis());

            foreach (var series in _diagramData.Series)
            {
                temp.Series.Add(InitializeLineSeries(series));
            }
            MyPlotModel = temp; // this is raising the INotifyPropertyChanged event			
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _diagramData.Caption;
        }

        private static LinearAxis DefineRateAxis()
        {
            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
//                Title = "Value",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dash
            };
            return yAxis;
        }

        private static DateTimeAxis DefineDateTimeAxis()
        {
            var xAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
//                StringFormat = Constants.MarketData.DisplayDateFormat,
//                Title = "End of Day",
                IntervalLength = 75,
                MinorIntervalType = DateTimeIntervalType.Days,
                IntervalType = DateTimeIntervalType.Days,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dash,
            };
            return xAxis;
        }

        private Series InitializeLineSeries(DiagramSeries series)
        {
            var result = new LineSeries {Title = series.Name};
//            result.Color = series.PositiveBrushColor;
            foreach (var point in series.Points)
            {
                result.Points.Add(new DataPoint(DateTimeAxis.ToDouble(point.CoorXdate), point.CoorYdouble));
            }
            return result;
        }

    }
}
