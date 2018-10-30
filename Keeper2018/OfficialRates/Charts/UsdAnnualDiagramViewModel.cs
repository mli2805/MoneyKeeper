using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Keeper2018
{
    public class UsdAnnualDiagramViewModel : Screen
    {
        private List<OfficialRatesModel> _rates;

        public PlotModel MyPlotModel2015 { get; set; } = new PlotModel();
        public PlotModel MyPlotModel2016 { get; set; } = new PlotModel();
        public PlotModel MyPlotModel2017 { get; set; } = new PlotModel();
        public PlotModel MyPlotModel2018 { get; set; } = new PlotModel();


        public void Initalize(List<OfficialRatesModel> rates)
        {
            _rates = rates;
            MyPlotModel2015.Series.Add(OneYearOfUsd(2015));
            MyPlotModel2015.Axes.Add(new DateTimeAxis()
            {
                Minimum = DateTimeAxis.ToDouble(new DateTime(2015,1,1)),
                IntervalLength = 45, 
                IntervalType = DateTimeIntervalType.Days, 
                MajorGridlineStyle = LineStyle.Solid,
                Maximum = DateTimeAxis.ToDouble(new DateTime(2015,12,31)),
            });

            MyPlotModel2016.Series.Add(OneYearOfUsd(2016));
            MyPlotModel2016.Axes.Add(new DateTimeAxis()
            {
                Minimum = DateTimeAxis.ToDouble(new DateTime(2016,1,1)),
                IntervalLength = 45, 
                IntervalType = DateTimeIntervalType.Days, 
                MajorGridlineStyle = LineStyle.Solid,
                Maximum = DateTimeAxis.ToDouble(new DateTime(2016,12,31)),
            });

            MyPlotModel2017.Series.Add(OneYearOfUsd(2017));
            MyPlotModel2017.Axes.Add(new DateTimeAxis()
            {
                Minimum = DateTimeAxis.ToDouble(new DateTime(2017,1,1)),
                IntervalLength = 45, 
                IntervalType = DateTimeIntervalType.Days, 
                MajorGridlineStyle = LineStyle.Solid,
                Maximum = DateTimeAxis.ToDouble(new DateTime(2017,12,31)),
            });

            MyPlotModel2018.Series.Add(OneYearOfUsd(2018));
            MyPlotModel2018.Axes.Add(new DateTimeAxis()
            {
                Minimum = DateTimeAxis.ToDouble(new DateTime(2018,1,1)),
                IntervalLength = 45, 
                IntervalType = DateTimeIntervalType.Days, 
                MajorGridlineStyle = LineStyle.Solid,
                Maximum = DateTimeAxis.ToDouble(new DateTime(2018,12,31)),
            });
        }

        private LineSeries OneYearOfUsd(int year)
        {
            var result = new LineSeries() { Title = year.ToString() };
            foreach (var nbRbRateOnScreen in _rates.Where(r => r.Date.Year == year))
            {
                var rate = nbRbRateOnScreen.Date < new DateTime(2016, 7, 1)
                    ? nbRbRateOnScreen.TodayRates.NbRates.Usd.Value / 10000
                    : nbRbRateOnScreen.TodayRates.NbRates.Usd.Value;
                result.Points.Add(new DataPoint(DateTimeAxis.ToDouble(nbRbRateOnScreen.Date), rate));
            }
            return result;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Usd annual diagram";
        }
    }
}
