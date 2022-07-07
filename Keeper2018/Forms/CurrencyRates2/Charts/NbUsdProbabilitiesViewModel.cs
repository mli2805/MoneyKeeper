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
        private List<OfficialRatesModel> _rates;
        public PlotModel MyPlotModel { get; set; } = new PlotModel();

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _caption;
        }

        public void Initialize(string caption, List<OfficialRatesModel> rates)
        {
            _caption = caption;
            _rates = rates;

            MyPlotModel.Series.Add(CreateStemSeries());
        }

        public List<string> ColumnLabels = new List<string>();

        private StemSeries CreateStemSeries()
        {
            var result = new StemSeries() { Color = OxyColors.Blue, };
            var startDate = new DateTime(2015, 1, 1);
            for (int i = 0; i < 365; i++)
            {
                var day = startDate.AddDays(i);
                var probability = 0.0;

                var days = _rates.Where(r => r.Date.Month == day.Month && r.Date.Day == day.Day && r.Date.Year >= 2015).ToArray();
                foreach (var currencyRatesModel in days)
                {
                    var previousModel = _rates.First(r => r.Date == currencyRatesModel.Date.AddDays(-1));
                    probability += Compare(currencyRatesModel, previousModel);
                }
                result.Points.Add(new DataPoint(Axis.ToDouble(i), probability));
            }
            return result;
        }

        private readonly DateTime _denomDay = new DateTime(2016, 7, 1);
        private double Compare(OfficialRatesModel day, OfficialRatesModel prevDay)
        {
            var dayRate = day.Date >= _denomDay
                ? day.TodayRates.NbRates.Usd.Value
                : day.TodayRates.NbRates.Usd.Value / 10000;

            var prevRate = prevDay.Date >= _denomDay
                ? prevDay.TodayRates.NbRates.Usd.Value
                : prevDay.TodayRates.NbRates.Usd.Value / 10000;

            if (dayRate.Equals(prevRate)) return 0;
            return dayRate > prevRate ? -1 : 1;
        }
    }
}
