using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Keeper2018
{
    public class RusBelChartViewModel : Screen
    {
        private string _caption;
        private List<CurrencyRatesModel> _rates;

        public LineSeries RusSeries { get; set; } = new LineSeries() { Title = "RUS", Color = OxyColors.Red};
        public LineSeries BelSeries { get; set; } = new LineSeries() { Title = "BEL", Color = OxyColors.Green};
        public LineSeries RubBynSeries { get; set; } = new LineSeries() {Title = "RUB/BYN", Color = OxyColors.Blue};

        private bool _isRubBynVisible = true;
        public bool IsRubBynVisible
        {
            get => _isRubBynVisible;
            set
            {
                _isRubBynVisible = value;
                RubBynSeries.IsVisible = value;
                MyPlotModel.InvalidatePlot(true);
            }
        }

        public PlotModel MyPlotModel { get; set; } = new PlotModel();

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _caption;
        }

        public void Initialize(string caption, List<CurrencyRatesModel> rates)
        {
            _caption = caption;
            _rates = rates;
            CreateSeries(new DateTime(2016,7,1));
            MyPlotModel.Series.Add(RusSeries);
            MyPlotModel.Series.Add(BelSeries);
            MyPlotModel.Series.Add(RubBynSeries);
            MyPlotModel.Axes.Add(new DateTimeAxis()
            {
                Minimum = DateTimeAxis.ToDouble(new DateTime(2018,12,1)),
                IntervalLength = 60, 
                IntervalType = DateTimeIntervalType.Days, 
                MajorGridlineStyle = LineStyle.Solid,
                Maximum = DateTimeAxis.ToDouble(DateTime.Today.AddDays(1)),
            });

        }

        private void CreateSeries(DateTime startDate)
        {
            BelSeries.Points.Clear();
            RusSeries.Points.Clear();
            RubBynSeries.Points.Clear();
            foreach (var line in _rates.Where(r => r.Date >= startDate))
            {
                BelSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(line.Date), line.TodayRates.NbRates.Usd.Value * 30));
                RusSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(line.Date), line.TodayRates.CbrRate.Usd.Value));
                RubBynSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(line.Date), line.TodayRates.NbRates.Rur.Value * 20));
            }
        }

    }
}
