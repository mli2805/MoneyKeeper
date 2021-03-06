using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Keeper2018
{
    public class MonthlyChartViewModel : Screen
    {
        private string _caption;
        private List<CurrencyRatesModel> _rates;
        public PlotModel MyPlotModel00 { get; set; } = new PlotModel();
        public PlotModel MyPlotModel01 { get; set; } = new PlotModel();
        public PlotModel MyPlotModel10 { get; set; } = new PlotModel();
        public PlotModel MyPlotModel11 { get; set; } = new PlotModel();

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Мой курс не НБ  (" + _caption + ")";
        }

        public void Initialize(string caption, List<CurrencyRatesModel> rates)
        {
            _rates = rates;
            _caption = caption;

            var date = new DateTime(2020, 4, 1);
            do
            {
                MyPlotModel00.Series.Add(OneMonthOfUsd(date));
                date = date.AddMonths(1);
            } while (date < new DateTime(2020, 7, 1));
            // MyPlotModel00.Axes.Add(new LinearAxis(){ Minimum = 2.515, Maximum = 2.67 });

           do
            {
                MyPlotModel01.Series.Add(OneMonthOfUsd(date));
                date = date.AddMonths(1);
            } while (date < new DateTime(2020, 10, 1));
            // MyPlotModel01.Axes.Add(new LinearAxis(){ Minimum = 2.515, Maximum = 2.67 });

           do
            {
                MyPlotModel10.Series.Add(OneMonthOfUsd(date));
                date = date.AddMonths(1);
            } while (date < new DateTime(2021, 1, 1));
            MyPlotModel10.Axes.Add(new LinearAxis(){ Minimum = 2.515, Maximum = 2.67 });

            do
            {
                MyPlotModel11.Series.Add(OneMonthOfUsd(date));
                date = date.AddMonths(1);
            } while (date <= DateTime.Now);
            MyPlotModel11.Axes.Add(new LinearAxis(){ Minimum = 2.515, Maximum = 2.67 });
        }


        private LineSeries OneMonthOfUsd(DateTime start)
        {
            var year = start.Year;
            var month = start.Month;
            var result = new LineSeries() { Title = $"{start:MMM yyyy}" };
            foreach (var nbRbRateOnScreen in _rates.Where(r => r.Date.Year == year && r.Date.Month == month))
            {
                var rate = nbRbRateOnScreen.TodayRates.MyUsdRate.Value;
                if (rate > 0)
                    result.Points.Add(new DataPoint(nbRbRateOnScreen.Date.Day, rate));
            }
            return result;
        }

    }
}
