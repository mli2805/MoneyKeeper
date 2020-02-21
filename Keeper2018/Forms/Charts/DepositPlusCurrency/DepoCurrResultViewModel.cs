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
            var points = _depoPlusCurrencyProvider.Evaluate(2008).ToList();
            InitializePlotModel(points);
        }

        private void InitializePlotModel(List<DepoCurrencyData> points)
        {
            MonthlySaldoModel = new PlotModel();
            MonthlySaldoModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Dash });

            var series = new ColumnSeries() { Title = "Depo", FillColor = OxyColors.Blue, IsStacked = true,  };
            series.Items.AddRange(points.Select(p=>new ColumnItem((double)p.DepoRevenue)));
            MonthlySaldoModel.Series.Add(series);

            var series2 = new ColumnSeries() { Title = "Currencies", FillColor = OxyColors.Green, IsStacked = true  };
            series2.Items.AddRange(points.Select(p=>new ColumnItem((double)p.CurrencyRatesDifferrence)));
            MonthlySaldoModel.Series.Add(series2);

            MonthlySaldoModel.Axes.Add(new CategoryAxis(null, points.Select(p => p.Label).ToArray()));
        }

     

    }
}
