using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Keeper2018
{
    public class MonthlyChartViewModel : Screen
    {
        private string _caption;
        private KeeperDataModel _keeperDataModel;
        public PlotModel MyPlotModel00 { get; set; } = new PlotModel();
        public PlotModel MyPlotModel01 { get; set; } = new PlotModel();
        public PlotModel MyPlotModel10 { get; set; } = new PlotModel();
        public PlotModel MyPlotModel11 { get; set; } = new PlotModel();

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Курс продажи в обменниках, не НБРБ  (" + _caption + ")";
        }

        public void Initialize(string caption, KeeperDataModel keeperDataModel)
        {
            _caption = caption;
            _keeperDataModel = keeperDataModel;

            var startDate = DateTime.Now.AddMonths(-12).GetStartOfMonth();

            var l00 = GetSeriesByMonths(startDate, startDate.AddMonths(3), out double min, out double max);
            l00.ForEach(MyPlotModel00.Series.Add);
            MyPlotModel00.Axes.Add(new LinearAxis() { Minimum = min, Maximum = max });

            var l01 = GetSeriesByMonths(startDate.AddMonths(3), startDate.AddMonths(6), out min, out max);
            l01.ForEach(MyPlotModel01.Series.Add);
            MyPlotModel01.Axes.Add(new LinearAxis() { Minimum = min, Maximum = max });

            var l10 = GetSeriesByMonths(startDate.AddMonths(6), startDate.AddMonths(9), out min, out max);
            l10.ForEach(MyPlotModel10.Series.Add);
            MyPlotModel10.Axes.Add(new LinearAxis() { Minimum = min, Maximum = max });

            var l11 = GetSeriesByMonths(startDate.AddMonths(9), DateTime.Today, out min, out max);
            l11.ForEach(MyPlotModel11.Series.Add);
            MyPlotModel11.Axes.Add(new LinearAxis() { Minimum = min, Maximum = max });
        }


        private List<LineSeries> GetSeriesByMonths(DateTime start, DateTime end, out double min, out double max)
        {
            var result = new List<LineSeries>();
            min = double.MaxValue;
            max = 0;
            while (start < end)
            {
                var ls = OneMonthOfUsd(start, out double lmin, out double lmax);
                result.Add(ls);
                if (lmin < min) min = lmin;
                if (lmax > max) max = lmax;

                start = start.AddMonths(1);
            }

            return result;
        }

        private LineSeries OneMonthOfUsd(DateTime start, out double min, out double max)
        {
            var year = start.Year;
            var month = start.Month;
            var result = new LineSeries() { Title = $"{start:MMM yyyy}" };
            var lines = _keeperDataModel.ExchangeRates
                .Values.Where(r => r.Date.Year == year && r.Date.Month == month).ToList();
            foreach (var line in lines)
            {
                var rate = line.UsdToByn;
                if (rate > 0)
                    result.Points.Add(new DataPoint(line.Date.Day, rate));
            }

            max = lines.Max(r => r.UsdToByn);
            min = lines.Min(r => r.UsdToByn);
            return result;
        }

    }
}
