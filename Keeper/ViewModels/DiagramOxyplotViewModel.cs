using System;
using System.Collections.Generic;
using System.Composition;
using Caliburn.Micro;
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
    private int _currentYear;
    private int _currentMonth;
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
      _currentYear = DateTime.Today.Year;
      _currentMonth = DateTime.Today.Month;
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
      temp.Series.Add(InitializePieSeries(ExtractMonth(_currentYear, _currentMonth)));
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

    private IEnumerable<ExpensePartingDataElement> ExtractMonth(int year, int month)
    {
      return _diagramData.Where(a => a.Month == month && a.Year == year);
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
        var newDate = new DateTime(_currentYear, _currentMonth, 1).AddMonths(destination);
        _currentYear = newDate.Year;
        _currentMonth = newDate.Month;
      }
      if (_type == 1)
        _currentYear += destination;

      Title = string.Format("   {0:MMMM yyyy}",new DateTime(_currentYear, _currentMonth, 1));
      InitializeDiagram();
    }

  }
}
