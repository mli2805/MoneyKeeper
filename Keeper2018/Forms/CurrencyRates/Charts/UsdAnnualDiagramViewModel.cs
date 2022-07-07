using System;
using System.Linq;
using Caliburn.Micro;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Keeper2018
{
    public class UsdAnnualDiagramViewModel : Screen
    {
        private string _caption;
        private KeeperDataModel _keeperDataModel;

        // public PlotModel MyPlotModel2017 { get; set; } = new PlotModel();
        // public PlotModel MyPlotModel2018 { get; set; } = new PlotModel();
        public PlotModel MyPlotModel2019 { get; set; } = new PlotModel();
        public PlotModel MyPlotModel2020 { get; set; } = new PlotModel();
        public PlotModel MyPlotModel2021 { get; set; } = new PlotModel();
        public PlotModel MyPlotModel2022 { get; set; } = new PlotModel();

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _caption;
        }

        public void Initialize(string caption, KeeperDataModel keeperDataModel)
        {
            _caption = caption;
            _keeperDataModel = keeperDataModel;

            // InitializeYear(MyPlotModel2017, 2017);
            // InitializeYear(MyPlotModel2018, 2018);
            InitializeYear(MyPlotModel2019, 2019);
            InitializeYear(MyPlotModel2020, 2020);
            InitializeYear(MyPlotModel2021, 2021);
            InitializeYear(MyPlotModel2022, 2022);
        }

        private void InitializeYear(PlotModel yearPlotModel, int year)
        {
            yearPlotModel.Series.Add(OneYearOfUsd(year));
            yearPlotModel.Axes.Add(new DateTimeAxis()
            {
                Minimum = DateTimeAxis.ToDouble(new DateTime(year, 1, 1)),
                IntervalLength = 45,
                IntervalType = DateTimeIntervalType.Days,
                MajorGridlineStyle = LineStyle.Solid,
                Maximum = DateTimeAxis.ToDouble(new DateTime(year, 12, 31)),
            });
        }

        private LineSeries OneYearOfUsd(int year)
        {
            var result = new LineSeries() { Title = year.ToString() };
            foreach (var officialRates in _keeperDataModel.Rates.Values.Where(r => r.Date.Year == year))
            {
                var rate = officialRates.Date < new DateTime(2016, 7, 1)
                    ? officialRates.NbRates.Usd.Value / 10000
                    : officialRates.NbRates.Usd.Value;
                result.Points.Add(new DataPoint(DateTimeAxis.ToDouble(officialRates.Date), rate));
            }
            return result;
        }

    }
}
