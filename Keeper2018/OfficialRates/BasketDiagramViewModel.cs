using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Keeper2018
{
    public class BasketDiagramViewModel : Screen
    {
        private DateTime _startDate = new DateTime(2018,1,1);
        private List<NbRbRateOnScreen> _rates;

        public PlotModel BasketPlotModel { get; set; }
        public PlotModel BasketDeltaPlotModel { get; set; }

        public void Initalize(List<NbRbRateOnScreen> rates)
        {
            BasketPlotModel = new PlotModel();
            BasketDeltaPlotModel = new PlotModel();

            _rates = rates;
            GetLinesOfBasket(out LineSeries basket, out ColumnSeries delta);

            BasketPlotModel.Series.Add(basket);
            BasketPlotModel.Axes.Add(new DateTimeAxis()
            {
//                IntervalType = DateTimeIntervalType.Months,
                MajorGridlineStyle = LineStyle.Solid,
            });

            BasketDeltaPlotModel.Series.Add(delta);
     
            BasketDeltaPlotModel.Axes.Add(new CategoryAxis()
            {
                IsTickCentered = true,
                MajorStep = (DateTime.Today - _startDate).Days / 18.0,
                LabelFormatter = F,
            });

        }

        private string F(double day)
        {
            return (_startDate + TimeSpan.FromDays(day)).ToShortDateString();
        }
        private void GetLinesOfBasket(out LineSeries basket, out ColumnSeries basketDelta)
        {
            basket = new LineSeries() { Title = "Basket", TextColor = OxyColors.Orange};
            basketDelta = new ColumnSeries() { Title = "DeltaP", FillColor = OxyColors.Red, NegativeFillColor = OxyColors.Green};
            foreach (var nbRbRateOnScreen in _rates.Where(r => r.Date >= _startDate))
            {
                basket.Points.Add(new DataPoint(DateTimeAxis.ToDouble(nbRbRateOnScreen.Date), nbRbRateOnScreen.Basket));
                basketDelta.Items.Add(new ColumnItem(nbRbRateOnScreen.ProcBasketDelta));
            }
        }

    }
}
