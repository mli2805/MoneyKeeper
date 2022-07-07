using System;
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
        private KeeperDataModel _keeperDataModel;
        private int _firstYear = 2017;
        public PlotModel MyPlotModel { get; set; } = new PlotModel();

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "курс доллара по НБ РБ  ( " + _caption + ")";
        }

        public void Initialize(string caption, KeeperDataModel keeperDataModel)
        {
            _caption = caption;
            _keeperDataModel = keeperDataModel;

            for (int i = _firstYear; i <= DateTime.Now.Year; i++)
            {
                MyPlotModel.Series.Add(OneYearOfUsd(i));
                var minvalue = DateTimeAxis.ToDouble(new DateTime(_firstYear, 1, 1).AddDays(-1));
                var maxvalue = DateTimeAxis.ToDouble(new DateTime(_firstYear, 12, 31).AddDays(1));
                MyPlotModel.Axes.Add(new DateTimeAxis()
                {
                    Position = AxisPosition.Bottom, Minimum = minvalue, Maximum = maxvalue, StringFormat = "MMM/dd"
                });
            }
        }

        private LineSeries OneYearOfUsd(int year)
        {
            var minus = year - _firstYear;
            var result = new LineSeries() { Title = year.ToString() };
            foreach (var ratesLine in _keeperDataModel.OfficialRates.Values.Where(r => r.Date.Year == year))
            {
                var rate = ratesLine.NbRates.Usd.Value;
                result.Points.Add(new DataPoint(DateTimeAxis.ToDouble(ratesLine.Date.AddYears(-minus)), rate));
            }
            return result;
        }

    }
}
