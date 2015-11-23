using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Windows.Documents;
using Keeper.Utils.DiagramDomainModel;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Keeper.Utils.OxyPlots
{
    [Export]
    class RatesOxyplotBuilder
    {
        private readonly DiagramData _diagramData;
        private List<LineSeries> _allSeries;

        [ImportingConstructor]
        public RatesOxyplotBuilder(DiagramData diagramData)
        {
            _diagramData = diagramData;
            _allSeries = InitializeAllSeries();
        }

        private List<LineSeries> InitializeAllSeries()
        {
            var result = new List<LineSeries>();
            foreach (var series in _diagramData.Series)
            {
                var lineSeries = new LineSeries() { Title = series.Name, Color = series.OxyColor, Tag = series.Index};
                foreach (var point in series.Points)
                {
                    lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(point.CoorXdate), point.CoorYdouble));
                }
                result.Add(lineSeries);
            }
            return result;
        }
        public PlotModel Do(RatesDiagramContentModel model)
        {
            var plotModel = new PlotModel();

            plotModel.Axes.Add(DefineDateTimeAxis());
            AddSeries(model, ref plotModel);

            plotModel.LegendPlacement = LegendPlacement.Inside;
            plotModel.LegendPosition = LegendPosition.LeftTop;

            return plotModel;
        }
        private void AddSeries(RatesDiagramContentModel model, ref PlotModel plotModel)
        {
            if (model.IsCheckedUsdNbRb)
            {
                var lineSeries = _allSeries.First(s => (int)s.Tag == 0);
                lineSeries.YAxisKey = "UsdNbRb";
                plotModel.Series.Add(lineSeries);
                plotModel.Axes.Add(DefineVerticalAxis("UsdNbRb"));
            }
            if (model.IsCheckedEurNbRb) plotModel.Series.Add(_allSeries.First(s => (int)s.Tag == 1));
            if (model.IsCheckedRurNbRb) plotModel.Series.Add(_allSeries.First(s => (int)s.Tag == 2));
            if (model.IsCheckedBusketNbRb)
            {
                var lineSeries = _allSeries.First(s => (int)s.Tag == 3);
                lineSeries.YAxisKey = "BusketNbRb";
                plotModel.Series.Add(lineSeries);
                plotModel.Axes.Add(DefineVerticalAxis("BusketNbRb", 0, 7000));
            }
            if (model.IsCheckedMyUsd) plotModel.Series.Add(_allSeries.First(s => (int)s.Tag == 4));
            if (model.IsCheckedUsdEur) plotModel.Series.Add(_allSeries.First(s => (int)s.Tag == 5));
            if (model.IsCheckedRurUsd) plotModel.Series.Add(_allSeries.First(s => (int)s.Tag == 6));
        }
        private static LinearAxis DefineVerticalAxis(string key, double min, double max)
        {
            return new LinearAxis { Position = AxisPosition.Right, MajorGridlineStyle = LineStyle.Solid, Minimum = min, Maximum = max, Key = key };
            
        }
        private static LinearAxis DefineVerticalAxis(string key)
        {
            return new LinearAxis { Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Solid, Key = key };
        }
        private static DateTimeAxis DefineDateTimeAxis()
        {
            var xAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                IntervalLength = 75,
                MinorIntervalType = DateTimeIntervalType.Days,
                IntervalType = DateTimeIntervalType.Days,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dash,
            };
            return xAxis;
        }

    }
}
