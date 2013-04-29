using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  class RatesDiagramViewModel : Screen
  {
    private DrawingImage _imageSource;
    public List<CurrencyRate> Data { get; set; }
    public DrawingGroup DrawingGroup = new DrawingGroup();
    public DrawingImage ImageSource
    {
      get { return _imageSource; }
      set
      {
        if (Equals(value, _imageSource)) return;
        _imageSource = value;
        NotifyOfPropertyChange(() => ImageSource);
      }
    }

    public RatesDiagramViewModel()
    {
      Data = new List<CurrencyRate>();
      var currencyRate = new CurrencyRate {BankDay = new DateTime(2013,1,1), Currency = CurrencyCodes.BYR, Rate = 8000};
      Data.Add(currencyRate);
      currencyRate = new CurrencyRate() { BankDay = new DateTime(2013, 1, 2), Currency = CurrencyCodes.BYR, Rate = 8200 };
      Data.Add(currencyRate);
      currencyRate = new CurrencyRate() { BankDay = new DateTime(2013, 2, 1), Currency = CurrencyCodes.BYR, Rate = 8600 };
      Data.Add(currencyRate);
      currencyRate = new CurrencyRate() { BankDay = new DateTime(2013, 3, 1), Currency = CurrencyCodes.BYR, Rate = 8500 };
      Data.Add(currencyRate);

      DiagramBackground();
      HorizontalAxisMarkers();
      DiagramData();
      ImageSource = new DrawingImage(DrawingGroup);
    }

    private void DiagramData()
    {
      var line = new LineGeometry(new Point(50, 0), new Point(400, 200));

      var geometryGroup = new GeometryGroup();
      geometryGroup.Children.Add(line);

      var geometryDrawing = new GeometryDrawing();
      geometryDrawing.Geometry = geometryGroup;
      // Настраиваем перо
      geometryDrawing.Pen = new Pen(Brushes.LimeGreen, 1);

      DrawingGroup.Children.Add(geometryDrawing);
    }

    private void DiagramBackground()
    {
      var geometryDrawing = new GeometryDrawing();

      var rectGeometry = new RectangleGeometry {Rect = new Rect(0, 0, 700, 700)};
      geometryDrawing.Geometry = rectGeometry;
      geometryDrawing.Brush = Brushes.LightYellow;// Кисть закраски
      // Добавляем готовый слой в контейнер отображения
      DrawingGroup.Children.Add(geometryDrawing);

      var geometryDrawing2 = new GeometryDrawing();

      var rectGeometry2 = new RectangleGeometry { Rect = new Rect(50, 0, 650, 670) };
      geometryDrawing2.Geometry = rectGeometry2;
      geometryDrawing2.Pen = new Pen(Brushes.Black, 1);
      geometryDrawing2.Brush = Brushes.Azure;// Кисть закраски
      // Добавляем готовый слой в контейнер отображения
      DrawingGroup.Children.Add(geometryDrawing2);
    }
    private void HorizontalAxisMarkers()
    {
      var geometryGroup = new GeometryGroup();

      for (var i = 0; i < Data.Count; i++)
      {
        var mark = String.Format("{0:d/M/yyyy} ", Data[i].BankDay);
        var formattedText = new FormattedText(mark, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, 
                                              new Typeface("Times New Roman"), 12, Brushes.Black);
        Geometry geometry = formattedText.BuildGeometry(new Point(i * 150.0 + 25, 685.0));
        geometryGroup.Children.Add(geometry);
      }

      var geometryDrawing = new GeometryDrawing();
      geometryDrawing.Geometry = geometryGroup;

      geometryDrawing.Pen = new Pen(Brushes.Black, 1);

      DrawingGroup.Children.Add(geometryDrawing);

    }


  }
}
