using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using OxyPlot;
using OxyPlot.Series;

namespace Keeper2018
{
    public class MonthlyChartViewModel : Screen
    {
        private string _caption;
        private List<CurrencyRatesModel> _rates;
        public PlotModel MyPlotModel { get; set; } = new PlotModel();
        public PlotModel MyPlotModel2 { get; set; } = new PlotModel();

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Мой курс не НБ  (" + _caption + ")";
        }

        public void Initialize(string caption, List<CurrencyRatesModel> rates)
        {
            _rates = rates;
            _caption = caption;

            var date = new DateTime(2020, 9, 1);
            do
            {
                MyPlotModel.Series.Add(OneMonthOfUsd(date));
                date = date.AddMonths(1);
            } while (date < new DateTime(2020, 12, 1));

            do
            {
                MyPlotModel2.Series.Add(OneMonthOfUsd(date));
                date = date.AddMonths(1);
            } while (date <= DateTime.Now);
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
