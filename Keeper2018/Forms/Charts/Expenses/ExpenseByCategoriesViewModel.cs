using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using OxyPlot;
using OxyPlot.Series;

namespace Keeper2018
{
    public class ExpenseByCategoriesModel : PropertyChangedBase
    {

    }
    public class ExpenseByCategoriesViewModel : Screen
    {
        private readonly KeeperDb _db;
        private readonly CategoriesDataExtractor _categoriesDataExtractor;
        private PeriodChoiceControlPointsConvertor _periodChoiceControlPointsConvertor;
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

        private double _fromPoint;
        public double FromPoint
        {
            get => _fromPoint;
            set
            {
                if (value.Equals(_fromPoint)) return;
                _fromPoint = value;
                SelectedInterval = _periodChoiceControlPointsConvertor.PointsToYearMonth(_fromPoint, _toPoint);
                NotifyOfPropertyChange();
            }
        }

        private double _toPoint;
        public double ToPoint
        {
            get => _toPoint;
            set
            {
                if (value.Equals(_toPoint)) return;
                _toPoint = value;
                SelectedInterval = _periodChoiceControlPointsConvertor.PointsToYearMonth(_fromPoint, _toPoint);
                NotifyOfPropertyChange();
            }
        }
        
        private Tuple<YearMonth, YearMonth> _selectedInterval;
        public Tuple<YearMonth, YearMonth> SelectedInterval
        {
            get => _selectedInterval;
            set
            {
                _selectedInterval = value;
                SelectedPeriodTitle = YearMonth.IntervalToString(value);
                InitializeDiagram();
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
       
        private string _selectedPeriodTitle;
        public string SelectedPeriodTitle
        {
            get => _selectedPeriodTitle;
            set
            {
                if (value == _selectedPeriodTitle) return;
                _selectedPeriodTitle = value;
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
            _periodChoiceControlPointsConvertor = 
                new PeriodChoiceControlPointsConvertor(_fullData.Min().YearMonth, _fullData.Max().YearMonth, DiagramIntervalMode.Months);
            SelectedInterval = new Tuple<YearMonth, YearMonth>(new YearMonth(DateTime.Today.AddMonths(-12)),new YearMonth(DateTime.Today));
            InitializeDiagram();
        }

        private void InitializeDiagram()
        {
            var temp = new PlotModel();
            var pieData = Extract(SelectedInterval).ToList();
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

        private IEnumerable<CategoriesDataElement> Extract(Tuple<YearMonth, YearMonth> period)
        {
            var r = _fullData.Where(a => a.YearMonth.InInterval(period))
                .GroupBy(e => e.CategoryId)
                .Select(g => new CategoriesDataElement(g.First().CategoryId,g.Sum(p=>p.Amount),
                    g.First().YearMonth)).Where(k => k.Amount > 0).OrderByDescending(o => o.Amount);
            return r;
        }

        public void PreviousPeriod()
        {

        } 
        public void NextPeriod()
        {

        }
    }
}
