using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Caliburn.Micro;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using DateTimeAxis = OxyPlot.Axes.DateTimeAxis;
using LinearAxis = OxyPlot.Axes.LinearAxis;

namespace Keeper2018
{
    public class FuelViewModel : Screen
    {
        private readonly KeeperDb _db;
        public List<Fuelling> Rows { get; set; }
        public string Total => $"Итого {Rows.Sum(f => f.Volume)} литров";

        public PlotModel ChartModel { get; set; }

        private Visibility _tableVisibility = Visibility.Collapsed;
        public Visibility TableVisibility
        {
            get => _tableVisibility;
            set
            {
                if (value == _tableVisibility) return;
                _tableVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private Visibility _chartVisibility = Visibility.Visible;
        public Visibility ChartVisibility
        {
            get => _chartVisibility;
            set
            {
                if (value == _chartVisibility) return;
                _chartVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public FuelViewModel(KeeperDb db)
        {
            _db = db;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Автомобильное топливо;  T - Toggle mode";
        }

        public void Initialize()
        {
            ExtractFuellingsFromDb();
            Rows = _db.Bin.Fuellings;

            InitializeChartModel();
        }

        private void InitializeChartModel()
        {
            ChartModel = new PlotModel();

            ChartModel.Axes.Add(new DateTimeAxis()
            {
                Position = AxisPosition.Bottom,
                IntervalLength = 75,
                MinorIntervalType = DateTimeIntervalType.Days,
                IntervalType = DateTimeIntervalType.Days,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dash,
            });
            ChartModel.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Automatic,
                MinorGridlineStyle = LineStyle.Automatic,
            });

            var dieselSeries = new LineSeries()
            { Title = "Дт Евро5", Color = OxyColors.BlueViolet };
            foreach (var fuelling in _db.Bin.Fuellings.Where(f => f.FuelType == FuelType.ДтЕвро5))
            {
                dieselSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(fuelling.Timestamp.Date), (double)fuelling.OneLitreInUsd));
            }
            ChartModel.Series.Add(dieselSeries);

            var arkticSeries = new ScatterSeries() {Title = "Дт Арктика", MarkerType  = MarkerType.Circle, MarkerFill = OxyColors.Red };
            foreach (var fuelling in _db.Bin.Fuellings.Where(f => f.FuelType == FuelType.ДтЕвро5Арктика))
            {
                arkticSeries.Points.Add(new ScatterPoint(DateTimeAxis.ToDouble(fuelling.Timestamp.Date), (double)fuelling.OneLitreInUsd));
            }
            ChartModel.Series.Add(arkticSeries);
        }

        private void ExtractFuellingsFromDb()
        {
            var trs = _db.Bin.Transactions.Values.Where(t => t.Tags.Contains(718) || t.Tags.Contains(714));
            _db.Bin.Fuellings = new List<Fuelling>();
            foreach (var tr in trs)
            {
                var volume = GetVolumeFromComment(tr.Comment);
                var oneLitrePrice = Math.Abs(volume) < 0.01 ? 0 : tr.Amount / (decimal)volume;
                var fuelling = new Fuelling()
                {
                    Timestamp = tr.Timestamp,
                    CarAccountId = tr.Tags.Contains(718) ? 716 : 711,
                    Amount = tr.Amount,
                    Currency = tr.Currency,
                    Volume = volume,
                    FuelType = GetFuelTypeFromComment(tr.Comment),
                    Comment = tr.Comment,

                    OneLitrePrice = oneLitrePrice,
                    OneLitreInUsd = _db.AmountInUsd(tr.Timestamp, tr.Currency, oneLitrePrice),
                };
                _db.Bin.Fuellings.Add(fuelling);
            }
            Console.WriteLine($@"{_db.Bin.Fuellings.Count} заправок, {_db.Bin.Fuellings.Sum(f => f.Volume)} литров");
        }

        private FuelType GetFuelTypeFromComment(string comment)
        {
            if (comment.Contains("керосин"))
                return FuelType.Керосин;
            if (comment.ToLowerInvariant().Contains("арктика"))
                return FuelType.ДтЕвро5Арктика;
            if (comment.ToLowerInvariant().Contains("castrol"))
                return FuelType.CastrolDTA;
            return FuelType.ДтЕвро5;
        }

        private double GetVolumeFromComment(string comment)
        {
            if (comment.StartsWith("Дт Евро5"))
                comment = comment.Substring(8);
            var resultString = Regex.Match(comment, "[+-]?([0-9]*[,])?[0-9]+").Value;
            if (string.IsNullOrEmpty(resultString))
                return 0;
            var result = double.TryParse(resultString, out double volume);
            return result ? volume : 0;
        }

        public void ToggleMode()
        {
            if (_tableVisibility == Visibility.Visible)
            {
                TableVisibility = Visibility.Collapsed;
                ChartVisibility = Visibility.Visible;
            }
            else
            {
                TableVisibility = Visibility.Visible;
                ChartVisibility = Visibility.Collapsed;

            }
        }

    }
}
