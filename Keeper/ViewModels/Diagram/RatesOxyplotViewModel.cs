using Caliburn.Micro;
using Keeper.Utils.DiagramDomainModel;
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

        public RatesOxyplotViewModel(DiagramData diagramData)
        {
            _diagramData = diagramData;
            InitializeDiagram();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _diagramData.Caption;
        }

        private void InitializeDiagram()
        {
            var temp = new PlotModel();
            foreach (var series in _diagramData.Series)
            {
                temp.Series.Add(InitializeLineSeries(series));
            }
            MyPlotModel = temp; // this is raising the INotifyPropertyChanged event			
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
