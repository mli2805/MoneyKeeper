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
    public Period SelectedInterval
    {
      get { return _selectedInterval; }
      set
      {
        _selectedInterval = value;
        TitleInterval = value;
        InitializeDiagram();
      }
    }

    public Period TitleInterval { get; set; }

    public int IntervalMode { get; set; }

    private readonly List<ExpensePartingDataElement> _diagramData;
    private PlotModel _myPlotModel;
    private Period _selectedInterval;

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
      SelectedInterval = new Period(DateTime.Today.GetStartOfMonthForDate(), DateTime.Today.GetEndOfDate());
      IntervalMode = 2; // month

      InitializeDiagram();
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = "Распределение расходов";
    }

    private void InitializeDiagram()
    {
      var temp = new PlotModel();
      temp.Series.Add(InitializePieSeries(Extract(SelectedInterval)));
      MyPlotModel = temp; // this is raising the INotifyPropertyChanged event			
    }

    private Series InitializePieSeries(IEnumerable<ExpensePartingDataElement> pieData)
    {
      var series = new PieSeries();
      foreach (var element in pieData)
      {
        series.Slices.Add(new PieSlice(element.Kategory.Name, (double)element.Amount));
      }
      return series;
    }

    private IEnumerable<ExpensePartingDataElement> Extract(Period period)
    {
      return _diagramData.Where(a => period.ContainsAndTimeWasChecked(new DateTime(a.Year, a.Month, 15)));
    }

    public void PreviousPeriod()
    {
      var copy = (Period) SelectedInterval.Clone();
      if (IntervalMode == 2) copy.MonthBack();
      else copy.YearBack();
      SelectedInterval = copy;
    }

    public void NextPeriod()
    {
      if (IntervalMode == 2) SelectedInterval.MonthForward();
      else SelectedInterval.YearForward();
    }

  }
}
