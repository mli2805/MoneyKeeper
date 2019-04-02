﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using OxyPlot;
using OxyPlot.Series;

namespace Keeper2018
{
    public class ExpenseByCategoriesViewModel : Screen
    {
        private readonly KeeperDb _db;
        private readonly CategoriesDataExtractor _categoriesDataExtractor;
        private List<CategoriesDataElement> _fullData;
     
        private PlotModel _myPlotModel;
        public PlotModel MyPlotModel
        {
            get => _myPlotModel;
            set
            {
                if (Equals(value, _myPlotModel)) return;
                _myPlotModel = value;
                NotifyOfPropertyChange();
            }
        }

        private ObservableCollection<string> _legendBindingSource;
        public ObservableCollection<string> LegendBindingSource
        {
            get => _legendBindingSource;
            set
            {
                if (Equals(value, _legendBindingSource)) return;
                _legendBindingSource = value;
                NotifyOfPropertyChange(() => LegendBindingSource);
            }
        }
   
        private Tuple<DateTime, DateTime> _myPeriod;
        public Tuple<DateTime, DateTime> MyPeriod
        {
            get => _myPeriod;
            set
            {
                if (Equals(value, _myPeriod)) return;
                _myPeriod = value;
                InitializeDiagram();
            }
        }

        private DiagramIntervalMode _myIntervalMode;
        public DiagramIntervalMode MyIntervalMode
        {
            get => _myIntervalMode;
            set
            {
                if (value == _myIntervalMode) return;
                _myIntervalMode = value;
                NotifyOfPropertyChange();
            }
        }

        public ExpenseByCategoriesViewModel(KeeperDb db, CategoriesDataExtractor categoriesDataExtractor)
        {
            _db = db;
            _categoriesDataExtractor = categoriesDataExtractor;
        }
        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Распределение расходов";
        }

        public void Initialize()
        {
            _fullData = _categoriesDataExtractor.GetExpenseGrouppedByCategoryAndMonth();
            MyIntervalMode = DiagramIntervalMode.Months;
            MyPeriod = new Tuple<DateTime, DateTime>(new DateTime(2002,1,1), DateTime.Today);
            InitializeDiagram();
        }

        private void InitializeDiagram()
        {
            var temp = new PlotModel();
            var pieData = Extract(MyPeriod).ToList();
            temp.Series.Add(InitializePieSeries(pieData));
            MyPlotModel = temp; // this is raising the INotifyPropertyChanged event			
            LegendBindingSource = InitializeLegend(pieData);
        }

        private Series InitializePieSeries(IEnumerable<CategoriesDataElement> pieData)
        {
            var series = new PieSeries();
            foreach (var element in pieData)
            {
                var category = _db.AcMoDict[element.CategoryId];
                series.Slices.Add(new PieSlice(category.Name, (double)element.Amount));
            }
            return series;
        }

        private ObservableCollection<string> InitializeLegend(List<CategoriesDataElement> pieData)
        {
            var result = new ObservableCollection<string>();
            var sum = pieData.Sum(a => a.Amount);
            foreach (var element in pieData)
            {
                var category = _db.AcMoDict[element.CategoryId];
                result.Add(
                    $"{category.Name} - {Math.Round(element.Amount/sum*100, 0)}%  (${Math.Round(element.Amount, 0):0,0})");
            }
            result.Add($"Всего  ${Math.Round(sum):0,0}");
            return result;
        }

        private IEnumerable<CategoriesDataElement> Extract(Tuple<DateTime, DateTime> period)
        {
            var periodYm = new Tuple<YearMonth, YearMonth>(new YearMonth(period.Item1), new YearMonth(period.Item2));

            var r = _fullData.Where(a => a.YearMonth.InInterval(periodYm))
                .GroupBy(e => e.CategoryId)
                .Select(g => new CategoriesDataElement(g.First().CategoryId,g.Sum(p=>p.Amount),
                    g.First().YearMonth)).Where(k => k.Amount > 0).OrderByDescending(o => o.Amount);
            return r;
        }
    }
}