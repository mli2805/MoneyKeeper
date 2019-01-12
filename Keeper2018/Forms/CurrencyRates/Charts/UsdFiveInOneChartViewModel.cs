using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Keeper2018
{
    public class UsdFiveInOneChartViewModel : Screen
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
            _rates = rates;
            _caption = caption;
        //    MyPlotModel.Series.Add(OneYearOfUsd(2015));
            MyPlotModel.Series.Add(OneYearOfUsd(2016));
            MyPlotModel.Series.Add(OneYearOfUsd(2017));
            MyPlotModel.Series.Add(OneYearOfUsd(2018));
            MyPlotModel.Series.Add(OneYearOfUsd(2019));

         
        }

        private LineSeries OneYearOfUsd(int year)
        {
            var minus = new DateTime(year, 1, 1);
            var result = new LineSeries() { Title = year.ToString() };
            foreach (var nbRbRateOnScreen in _rates.Where(r => r.Date.Year == year))
            {
                var rate = nbRbRateOnScreen.Date < new DateTime(2016, 7, 1)
                    ? nbRbRateOnScreen.TodayRates.NbRates.Usd.Value / 10000
                    : nbRbRateOnScreen.TodayRates.NbRates.Usd.Value;
                result.Points.Add(new DataPoint(Axis.ToDouble(nbRbRateOnScreen.Date - minus.Date), rate));
            }
            return result;
        }

    }
}
