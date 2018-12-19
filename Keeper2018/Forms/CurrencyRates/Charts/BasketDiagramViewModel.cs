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
        private string _caption;
        private DateTime _startDate = new DateTime(2018, 1, 1);
        private List<CurrencyRatesModel> _rates;

        public PlotModel BasketPlotModel { get; set; }
        public PlotModel BasketDeltaPlotModel { get; set; }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _caption + "  ;  T - Transform";
        }

        public void Initalize(string caption, List<CurrencyRatesModel> rates)
        {
            _caption = caption;

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
            basket = new LineSeries() {Title = "Корзина 30-20-50", TextColor = OxyColors.Orange};
            basketDelta = new ColumnSeries()
            {
                Title = "Дневное изменение корзины в %",
                FillColor = OxyColors.Red,
                NegativeFillColor = OxyColors.Green
            };
            foreach (var nbRbRateOnScreen in _rates.Where(r => r.Date >= _startDate))
            {
                basket.Points.Add(new DataPoint(DateTimeAxis.ToDouble(nbRbRateOnScreen.Date), nbRbRateOnScreen.Basket));
                basketDelta.Items.Add(new ColumnItem(nbRbRateOnScreen.ProcBasketDelta));
            }
        }


        #region Transform chart

        private int _rowB = 1;
        private int _rowSpan = 1;
        private int _columnB;
        private int _columnSpan = 2;

        public int RowB
        {
            get { return _rowB; }
            set
            {
                if (value == _rowB) return;
                _rowB = value;
                NotifyOfPropertyChange();
            }
        }

        public int RowSpan
        {
            get { return _rowSpan; }
            set
            {
                if (value == _rowSpan) return;
                _rowSpan = value;
                NotifyOfPropertyChange();
            }
        }

        public int ColumnB
        {
            get { return _columnB; }
            set
            {
                if (value == _columnB) return;
                _columnB = value;
                NotifyOfPropertyChange();
            }
        }

        public int ColumnSpan
        {
            get { return _columnSpan; }
            set
            {
                if (value == _columnSpan) return;
                _columnSpan = value;
                NotifyOfPropertyChange();
            }
        }

        public void TransformChart()
        {
            RowB = RowB == 1 ? 0 : 1;
            RowSpan = RowSpan == 1 ? 2 : 1;
            ColumnB = ColumnB == 1 ? 0 : 1;
            ColumnSpan = ColumnSpan == 1 ? 2 : 1;
        }

        #endregion
    }
}