using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.Utils.DiagramDomainModel;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Keeper.Utils.DiagramOxyPlots
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
            if (model.IsCheckedUsdNbRb) DefineLineSeries(0, "UsdNbRb", ref plotModel);
            if (model.IsCheckedEurNbRb) plotModel.Series.Add(_allSeries.First(s => (int)s.Tag == 1));
            if (model.IsCheckedRurNbRb) DefineLineSeries(2, "RurNbRb", ref plotModel);
            if (model.IsCheckedBusketNbRb) DefineLineSeries(3, "BusketNbRb", 0, 7000, ref plotModel);
            if (model.IsCheckedMyUsd) plotModel.Series.Add(_allSeries.First(s => (int)s.Tag == 4));
            if (model.IsCheckedEurUsdNbRb) DefineLineSeries(5, "EurUsdNbRb", ref plotModel);
            if (model.IsCheckedRurUsd) DefineLineSeries(6, "RurUsdNbRb", 0, 150, ref plotModel);
        }

        private void DefineLineSeries(int tag, string key, ref PlotModel plotModel)
        {
            var lineSeries = _allSeries.First(s => (int)s.Tag == tag);
            lineSeries.YAxisKey = key;
            plotModel.Series.Add(lineSeries);
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Solid, Key = key });
        }
        private void DefineLineSeries(int tag, string key, double min, double max, ref PlotModel plotModel)
        {
            var lineSeries = _allSeries.First(s => (int)s.Tag == tag);
            lineSeries.YAxisKey = key;
            plotModel.Series.Add(lineSeries);
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Right, MajorGridlineStyle = LineStyle.Solid, Minimum = min, Maximum = max, Key = key });
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
