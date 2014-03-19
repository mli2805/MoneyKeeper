using System;
using System.Collections.Generic;
using System.Composition;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils.Common;
using Keeper.Utils.OxyPlots;
using OxyPlot;
using OxyPlot.Series;
using System.Linq;

namespace Keeper.ViewModels
{
  [Export]
  internal class DiagramOxyplotViewModel : Screen
  {
    private readonly List<ExpensePartingDataElement> _diagramData;
    private int _currentYearFrom;
    private int _currentYearTo;
    private int _currentMonthFrom;
    private int _currentMonthTo;
    private int _type;
    private string _title;
    private PlotModel _myPlotModel;

    public string Title
    {
      get { return _title; }
      set
      {
        if (value == _title) return;
        _title = value;
        NotifyOfPropertyChange(() => Title);
      }
    }

    public PlotModel MyPlotModel
    {
      get { return _myPlotModel; }
      set
      {
        if (Equals(value, _myPlotModel)) return;
        _myPlotModel = value;
        NotifyOfPropertyChange(() => MyPlotModel);
      }
    }

    public DiagramOxyplotViewModel(List<ExpensePartingDataElement> diagramData)
    {
      _diagramData = diagramData;
      _currentYearFrom = _currentYearTo = DateTime.Today.Year;
      _currentMonthFrom = _currentMonthTo = DateTime.Today.Month;
      _type = 2; // month

      InitializeDiagram();
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = "Распределение расходов";
    }

    private void InitializeDiagram()
    {
      var temp = new PlotModel();
      temp.Series.Add(InitializePieSeries(Extract(new Period(new DateTime(_currentYearFrom,_currentMonthFrom,1), new DateTime(_currentYearTo, _currentMonthTo,1).GetEndOfMonthForDate()))));
      MyPlotModel = temp; // this is raising the INotifyPropertyChanged event			
    }

    private Series InitializePieSeries(IEnumerable<ExpensePartingDataElement> pieData)
    {
      var series = new PieSeries();
      foreach (var element in pieData)
      {
        series.Slices.Add(new PieSlice(element.Kategory.Name,(double)element.Amount));
      }
      return series;
    }

    private IEnumerable<ExpensePartingDataElement> Extract(Period period)
    {
      return _diagramData.Where(a => period.Contains(new DateTime(a.Year, a.Month, 15)));
    }

    public void PreviousPeriod()
    {
      ChangePeriod(-1);
    }

    public void NextPeriod()
    {
      ChangePeriod(1);
    }

    private void ChangePeriod(int destination)
    {
      if (_type == 2)
      {
        var newDate = new DateTime(_currentYearFrom, _currentMonthFrom, 1).AddMonths(destination);
        _currentYearFrom = newDate.Year;
        _currentMonthFrom = newDate.Month;
        newDate = new DateTime(_currentYearTo, _currentMonthTo, 1).AddMonths(destination);
        _currentYearTo = newDate.Year;
        _currentMonthTo = newDate.Month;
      }
      if (_type == 1)
        _currentYearFrom += destination;

      CombineTitle();
      InitializeDiagram();
    }

    private void CombineTitle()
    {
      if (_type == 2) // месяца
      {
        if (_currentYearFrom == _currentYearTo && _currentMonthFrom == _currentMonthTo)
          Title = string.Format("   {0:MMMM yyyy}", new DateTime(_currentYearFrom, _currentMonthFrom, 1));
        else
          Title = string.Format("   {0:MMMM yyyy}  {1:MMMM yyyy}",
                                    new DateTime(_currentYearFrom, _currentMonthFrom, 1),
                                    new DateTime(_currentYearTo, _currentMonthTo, 1));
      }
      if (_type == 1) // годы
      {
        if (_currentYearFrom == _currentYearTo)
          Title = string.Format("   {0} год", _currentYearFrom);
        else
          Title = string.Format("   {0} - {1} годы", _currentYearFrom, _currentYearTo);
      }

    }

  }
}
