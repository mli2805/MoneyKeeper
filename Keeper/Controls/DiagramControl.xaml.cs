using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Keeper.Utils;

namespace Keeper.Controls
{
  /// <summary>
  /// Interaction logic for DiagramControl.xaml
  /// </summary>
  public partial class DiagramControl : UserControl
  {
    public void Connect(int connectionId, object target)
    {
      throw new NotImplementedException();
    }

    public static readonly DependencyProperty AllDiagramDataProperty = 
      DependencyProperty.Register("AllDiagramData", typeof (List<List<DiagramPair>>), typeof (DiagramControl), new FrameworkPropertyMetadata(new List<List<DiagramPair>>()));

    public List<List<DiagramPair>> AllDiagramData
    {
      get { return (List<List<DiagramPair>>) GetValue(AllDiagramDataProperty); }
      set { SetValue(AllDiagramDataProperty, value); } 
    }

    public static readonly DependencyProperty SeriesTypeProperty = DependencyProperty.Register("SeriesType",
                                                                                               typeof (int),
                                                                                               typeof (DiagramControl), new FrameworkPropertyMetadata(0));
    public int SeriesType
    {
      get { return (int) GetValue(SeriesTypeProperty); }
      set { SetValue(SeriesTypeProperty, value);}
    }

    public List<List<DiagramPair>> CurrentDiagramData { get; set; }
    private DateTime _minDate, _maxDate;
    private double _minValue, _maxValue;
    private double LowestScaleValue { get { return _fromDivision * _accurateValuesPerDivision; } }

    public double ImageWidth { get { return IsLoaded ? ActualWidth : SystemParameters.FullPrimaryScreenWidth; } }
    public double ImageHeight { get { return IsLoaded ? (ActualHeight - StatusBar.ActualHeight) : SystemParameters.FullPrimaryScreenHeight; } }

    private double LeftMargin { get { return ImageWidth * 0.03; } }
    private double RightMargin { get { return ImageWidth * 0.03; } }
    private double TopMargin { get { return ImageHeight * 0.03; } }
    private double BottomMargin { get { return ImageHeight * 0.03; } }

    private double PointPerDate
    {
      get
      {
        if (CurrentDiagramData == null) return 0;
        if (CurrentDiagramData.Count == 0) return 0;
        return (ImageWidth - LeftMargin - RightMargin - Shift) / CurrentDiagramData.Count;
      }
    }
    private double Gap { get { return PointPerDate / 3; } } // промежуток между столбиками диаграммы
    private double PointPerBar { get { return PointPerDate - Gap; } }
    private double Shift { get { return ImageWidth * 0.002; } } // от левой оси до первого столбика

    //---------- vertical axes
    private double _accurateValuesPerDivision;
    private int _fromDivision;
    private int _divisions;
    private double PointPerScaleStep { get { return (ImageHeight - TopMargin - BottomMargin) / _divisions; } }

    private double PointPerOneValueAfter { get { return (ImageHeight - TopMargin - BottomMargin) / (_divisions * _accurateValuesPerDivision); } }
    private double Y0
    {
      get
      {
        var temp = ImageHeight - BottomMargin;
        if (_fromDivision < 0) temp += PointPerScaleStep * _fromDivision;
        return temp;
      }
    }

    public DrawingGroup DrawingGroup = new DrawingGroup();
    public DrawingImage ImageSource { get; set; }

    public DiagramControl()
    {
      InitializeComponent();
      this.Loaded += BarDiagramControlOnLoaded;
    }

    void BarDiagramControlOnLoaded(object sender, RoutedEventArgs e)
    {
      CurrentDiagramData = new List<List<DiagramPair>>(AllDiagramData);
      if (CurrentDiagramData.Count == 0) return;
      DrawCurrentDiagram();
      DiagramImage.Source = ImageSource;

      var window = Window.GetWindow(this);
//      if (window != null) window.KeyDown += OnKeyDown;
    }


    private void GetDiagramDataLimits()
    {
      _minDate = CurrentDiagramData[0][0].CoorXdate;
      _maxDate = CurrentDiagramData[0].Last().CoorXdate;
      _minValue = CurrentDiagramData[0].Min(r => r.CoorYdouble);
      _maxValue = CurrentDiagramData[0].Max(r => r.CoorYdouble);

      for (int i = 1; i < CurrentDiagramData.Count; i++)
      {
        if (_minDate > CurrentDiagramData[i][0].CoorXdate) _minDate = CurrentDiagramData[i][0].CoorXdate;
        if (_maxDate < CurrentDiagramData[i].Last().CoorXdate) _maxDate = CurrentDiagramData[i].Last().CoorXdate;
        if (_minValue > CurrentDiagramData[i].Min(r => r.CoorYdouble)) _minValue = CurrentDiagramData[i].Min(r => r.CoorYdouble);
        if (_maxValue < CurrentDiagramData[0].Max(r => r.CoorYdouble)) _maxValue = CurrentDiagramData[0].Max(r => r.CoorYdouble);
      }
    }

    #region Drawing implementation

    public void DrawCurrentDiagram()
    {
      GetDiagramDataLimits();

      DiagramBackground();

      HorizontalAxes();
      HorizontalAxisWithMarkers(Dock.Top);
      HorizontalAxisWithMarkers(Dock.Bottom);

      VerticalAxes();
      MarkersForVerticalAxes(Dock.Left);
      MarkersForVerticalAxes(Dock.Right);

      Diagram();
      ImageSource = new DrawingImage(DrawingGroup);
    }

    private void DiagramBackground()
    {
      var geometryDrawing = new GeometryDrawing();

      var rectGeometry = new RectangleGeometry { Rect = new Rect(0, 0, ImageWidth, ImageHeight) };
      geometryDrawing.Geometry = rectGeometry;
      geometryDrawing.Brush = Brushes.LightYellow; // Кисть закраски
      // Добавляем готовый слой в контейнер отображения
      DrawingGroup.Children.Add(geometryDrawing);

      var geometryDrawing2 = new GeometryDrawing();

      var rectGeometry2 = new RectangleGeometry
      {
        Rect = new Rect(LeftMargin, TopMargin, ImageWidth - LeftMargin - RightMargin,
                        ImageHeight - TopMargin - BottomMargin)
      };
      geometryDrawing2.Geometry = rectGeometry2;
      geometryDrawing2.Brush = Brushes.White; // Кисть закраски
      // Добавляем готовый слой в контейнер отображения
      DrawingGroup.Children.Add(geometryDrawing2);
    }

    private void HorizontalAxes()
    {
      var geometryGroup = new GeometryGroup();
      var bottomAxis = new LineGeometry(new Point(LeftMargin, ImageHeight - BottomMargin),
                                        new Point(ImageWidth - RightMargin, ImageHeight - BottomMargin));
      geometryGroup.Children.Add(bottomAxis);

      var topAxis = new LineGeometry(new Point(LeftMargin, TopMargin),
                                     new Point(ImageWidth - RightMargin, TopMargin));
      geometryGroup.Children.Add(topAxis);

      var geometryDrawing = new GeometryDrawing { Geometry = geometryGroup, Pen = new Pen(Brushes.Black, 1) };
      DrawingGroup.Children.Add(geometryDrawing);
    }

    private void HorizontalAxisWithMarkers(Dock flag)
    {

      const double minPointBetweenMarkedDivision = 50;
      var markedDash = (int)Math.Ceiling(minPointBetweenMarkedDivision / PointPerDate);

      var geometryGroupDashesAndMarks = new GeometryGroup();
      var geometryGroupGridlines = new GeometryGroup();

      for (var i = 0; i < CurrentDiagramData.Count; i++)
      {
        var dashY = flag == Dock.Bottom ? ImageHeight - BottomMargin : TopMargin;
        var dash = new LineGeometry(new Point(LeftMargin + Shift / 2 + PointPerDate * (i + 0.5), dashY - 5),
                                    new Point(LeftMargin + Shift / 2 + PointPerDate * (i + 0.5), dashY + 5));
        geometryGroupDashesAndMarks.Children.Add(dash);


        if (i % markedDash == 0)
        {
          var gridline = new LineGeometry(new Point(LeftMargin + Shift / 2 + PointPerDate * (i + 0.5) - 1, TopMargin + 5),
                                          new Point(LeftMargin + Shift / 2 + PointPerDate * (i + 0.5) - 1,
                                                    ImageHeight - BottomMargin - 5));
          geometryGroupGridlines.Children.Add(gridline);

          var markY = flag == Dock.Bottom ? ImageHeight : TopMargin;
          var mark = String.Format("{0:M/yyyy} ", CurrentDiagramData[0][i].CoorXdate);
          var formattedText = new FormattedText(mark, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                                                new Typeface("Times New Roman"), 12, Brushes.Black);
          var geometry =
            formattedText.BuildGeometry(new Point(LeftMargin + Shift / 2 + PointPerDate * (i + 0.5) - 18, markY - 20));
          geometryGroupDashesAndMarks.Children.Add(geometry);
        }
      }

      var geometryDrawingDashesAndMarks = new GeometryDrawing { Geometry = geometryGroupDashesAndMarks, Pen = new Pen(Brushes.Black, 1) };
      DrawingGroup.Children.Add(geometryDrawingDashesAndMarks);

      var geometryDrawingGridlines = new GeometryDrawing { Geometry = geometryGroupGridlines, Pen = new Pen(Brushes.LightGray, 1) };
      DrawingGroup.Children.Add(geometryDrawingGridlines);
    }

    private void VerticalAxes()
    {
      var geometryGroup = new GeometryGroup();
      var leftAxis = new LineGeometry(new Point(LeftMargin, TopMargin),
                                      new Point(LeftMargin, ImageHeight - BottomMargin));
      geometryGroup.Children.Add(leftAxis);
      var rightAxis = new LineGeometry(new Point(ImageWidth - RightMargin, TopMargin),
                                       new Point(ImageWidth - RightMargin, ImageHeight - BottomMargin));
      geometryGroup.Children.Add(rightAxis);

      var geometryDrawing = new GeometryDrawing { Geometry = geometryGroup, Pen = new Pen(Brushes.Black, 1) };
      DrawingGroup.Children.Add(geometryDrawing);
    }

    private void MarkersForVerticalAxes(Dock flag)
    {
      const double minPointBetweenDivision = 35;

      double pointPerOneValueBefore = _maxValue.Equals(_minValue) ? 0 : (ImageHeight - TopMargin - BottomMargin) / (_maxValue - _minValue);
      double valuesPerDivision = (minPointBetweenDivision > pointPerOneValueBefore) ?
                  Math.Ceiling(minPointBetweenDivision / pointPerOneValueBefore) : minPointBetweenDivision / pointPerOneValueBefore;
      double zeros = Math.Floor(Math.Log10(valuesPerDivision));
      _accurateValuesPerDivision = Math.Ceiling(valuesPerDivision / Math.Pow(10, zeros)) * Math.Pow(10, zeros);
      _fromDivision = Convert.ToInt32(Math.Floor(_minValue / _accurateValuesPerDivision));

      var temp = Convert.ToInt32(Math.Ceiling((_maxValue - _minValue) / _accurateValuesPerDivision));
      if ((_fromDivision + temp) * _accurateValuesPerDivision < _maxValue) temp++;
      _divisions = temp;

      var geometryGroupDashesAndMarks = new GeometryGroup();
      var geometryGroupGridlines = new GeometryGroup();
      for (var i = 0; i <= _divisions; i++)
      {
        var dashX = flag == Dock.Left ? LeftMargin : ImageWidth - LeftMargin;
        var dash = new LineGeometry(new Point(dashX - 5, ImageHeight - BottomMargin - PointPerScaleStep * i),
                                    new Point(dashX + 5, ImageHeight - BottomMargin - PointPerScaleStep * i));
        geometryGroupDashesAndMarks.Children.Add(dash);

        var gridline = new LineGeometry(new Point(LeftMargin + 5, ImageHeight - BottomMargin - PointPerScaleStep * i),
                                        new Point(ImageWidth - LeftMargin - 5,
                                                  ImageHeight - BottomMargin - PointPerScaleStep * i));
        if (i + _fromDivision == 0)
          geometryGroupDashesAndMarks.Children.Add(gridline);
        else if (i != 0 && i != _divisions) geometryGroupGridlines.Children.Add(gridline);

        var markX = flag == Dock.Left ? LeftMargin - 40 : ImageWidth - RightMargin + 15;
        var mark = String.Format("{0} ", (i + _fromDivision) * _accurateValuesPerDivision);
        var formattedText = new FormattedText(mark, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                                              new Typeface("Times New Roman"), 12, Brushes.Black);
        var geometry =
          formattedText.BuildGeometry(new Point(markX, ImageHeight - BottomMargin - PointPerScaleStep * i - 7));
        geometryGroupDashesAndMarks.Children.Add(geometry);
      }

      var geometryDrawingDashesAndMarks = new GeometryDrawing { Geometry = geometryGroupDashesAndMarks, Pen = new Pen(Brushes.Black, 1) };
      DrawingGroup.Children.Add(geometryDrawingDashesAndMarks);

      var geometryDrawingGridlines = new GeometryDrawing { Geometry = geometryGroupGridlines, Pen = new Pen(Brushes.LightGray, 1) };
      DrawingGroup.Children.Add(geometryDrawingGridlines);
    }


    private void Diagram()
    {
      foreach (var series in CurrentDiagramData)
      {
         if (SeriesType == 0) DrawLineSeries(series);
         else DrawBarSeries(series);
      }  
    }

    private void DrawBarSeries(List<DiagramPair> series)
    {
      
    }
    private void DrawLineSeries(List<DiagramPair> series)
    {
      var geometryGroupPositive = new GeometryGroup();
      var geometryGroupNegative = new GeometryGroup();
      for (int i = 0; i < series.Count; i++)
      {
        Rect rect;
        if (series[i].CoorYdouble >= 0)
          rect = new Rect(LeftMargin + Shift / 2 + Gap / 2 + i * (PointPerBar + Gap),
                          ImageHeight - BottomMargin -
                          (series[i].CoorYdouble - LowestScaleValue) * PointPerOneValueAfter,
                          PointPerBar,
                          series[i].CoorYdouble * PointPerOneValueAfter);
        else
          rect = new Rect(LeftMargin + Shift / 2 + Gap / 2 + i * (PointPerBar + Gap),
                          ImageHeight - BottomMargin - (0 - LowestScaleValue) * PointPerOneValueAfter,
                          PointPerBar,
                          -series[i].CoorYdouble * PointPerOneValueAfter);

        var rectangleGeometry = new RectangleGeometry(rect);
        if (series[i].CoorYdouble > 0)
          geometryGroupPositive.Children.Add(rectangleGeometry);
        else
          geometryGroupNegative.Children.Add(rectangleGeometry);

      }

      var geometryDrawingPositive = new GeometryDrawing
      {
        Geometry = geometryGroupPositive,
        Pen = new Pen(Brushes.Blue, 1),
        Brush = Brushes.Blue
      };
      DrawingGroup.Children.Add(geometryDrawingPositive);
      var geometryDrawingNegative = new GeometryDrawing
      {
        Geometry = geometryGroupNegative,
        Pen = new Pen(Brushes.Red, 1),
        Brush = Brushes.Red
      };
      DrawingGroup.Children.Add(geometryDrawingNegative);
    }

    #endregion
  }
}
