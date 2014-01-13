using System;
using System.Composition;
using Caliburn.Micro;
using Keeper.Utils.Diagram;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Keeper.ViewModels
{
  [Export]
  class DiagramOxyplotViewModel : Screen
  {
    private readonly DiagramData _diagramData;
    public PlotModel MyPlotModel { get; set; }

    public DiagramOxyplotViewModel(DiagramData diagramData)
    {
      _diagramData = diagramData;
      InitializeModel();
    }

    private void InitializeModel()
    {
      var temp = new PlotModel(_diagramData.Caption);

      foreach (var diagramSeries in _diagramData.Data)
      {
        temp.Series.Add(InitializeColumnSeries(diagramSeries));
      }
      temp.Axes.Add(new LinearAxis(AxisPosition.Left));
      temp.Axes.Add(new CategoryAxis(AxisPosition.Bottom));
      MyPlotModel = temp; // this is raising the INotifyPropertyChanged event			
    }



    private Series InitializeLinearSeries(DiagramSeries diagramSeries)
    {
      var ls = new LineSeries();

      foreach (var diagramPair in diagramSeries.Data)
      {
        ls.Points.Add(new DataPoint(DateTimeAxis.ToDouble(diagramPair.CoorXdate), (int)diagramPair.CoorYdouble));
      }
      return ls;
    }
    private Series InitializeColumnSeries(DiagramSeries diagramSeries)
    {
      var ls = new ColumnSeries();

      foreach (var diagramPair in diagramSeries.Data)
      {
        ls.Items.Add(new ColumnItem((int)diagramPair.CoorYdouble));
      }
      return ls;
    }
  }
}
