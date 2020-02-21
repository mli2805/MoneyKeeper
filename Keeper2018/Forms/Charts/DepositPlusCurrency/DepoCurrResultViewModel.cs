using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Keeper2018
{
    public class DepoCurrResultViewModel : Screen
    {
        public PlotModel MonthlySaldoModel { get; set; }

        private readonly DepoPlusCurrencyProvider _depoPlusCurrencyProvider;

        public DepoCurrResultViewModel(DepoPlusCurrencyProvider depoPlusCurrencyProvider)
        {
            _depoPlusCurrencyProvider = depoPlusCurrencyProvider;
        }

        public void Initialize()
        {
            _depoPlusCurrencyProvider.Initialize();
            var points = _depoPlusCurrencyProvider.Evaluate(2016).ToList();
            InitializePlotModel(points);
        }

        private void InitializePlotModel(List<DepoCurrencyData> points)
        {
            MonthlySaldoModel = new PlotModel();
            MonthlySaldoModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Dash });
            MonthlySaldoModel.Series.Add(Convert(points));
            MonthlySaldoModel.Axes.Add(new CategoryAxis(null, points.Select(p => p.Label).ToArray()));
        }

        public ColumnSeries Convert(IEnumerable<DepoCurrencyData> points)
        {
            var monthlySeries = new ColumnSeries() { Title = ""};
            foreach (var point in points)
            {
                monthlySeries.Items.Add(new ColumnItem((double)point.Saldo));
            }

            return monthlySeries;
        }
    }
}
