using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Keeper2018
{
    public class NbUsdProbabilitiesViewModel : Screen
    {
        private string _caption;
        private List<CurrencyRatesModel> _rates;
        public PlotModel MyPlotModel { get; set; } = new PlotModel();

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _caption;
        }

        public void Initialize(string caption, List<CurrencyRatesModel> rates)
        {
            _caption = caption;
            _rates = rates;
            MyPlotModel.Series.Add(CreateSeries());
            MyPlotModel.Axes.Add(new DateTimeAxis()
            {
                Minimum = DateTimeAxis.ToDouble(new DateTime(2015,1,1)),
                IntervalLength = 60, 
                IntervalType = DateTimeIntervalType.Days, 
                MajorGridlineStyle = LineStyle.Solid,
                Maximum = DateTimeAxis.ToDouble(new DateTime(2015,12,31)),
            });

        }

        public LineSeries CreateSeries()
        {
            var lineSeries = new LineSeries();
            var startDate = new DateTime(2015,1,1);
            for (int i = 0; i < 365; i++)
            {
                var day = startDate.AddDays(i);
                var previousDay = day.AddDays(-1);
                var probability = 0.0;
               
                var days = _rates.Where(r => r.Date.Month == day.Month && r.Date.Day == day.Day && r.Date.Year >= 2015).
                    Select(l=>l.TodayRates.NbRates.Usd.Value).ToArray();
                var previousDays = _rates.Where(r => r.Date.Month == previousDay.Month && r.Date.Day == previousDay.Day && r.Date.Year >= 2015).
                    Select(l=>l.TodayRates.NbRates.Usd.Value).Take(days.Length).ToArray();
                for (int j = 0; j < days.Length; j++)
                {
                    probability = days[j] < previousDays[j] ? probability + 1 : probability - 1;
                    
                }
                lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(day.Date), probability / days.Length));
                
            }
            return lineSeries;
        }
    }
}
