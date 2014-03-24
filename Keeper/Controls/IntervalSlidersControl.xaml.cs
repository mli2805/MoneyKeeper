using System;
using System.Windows;
using System.Windows.Controls;
using Keeper.DomainModel;

namespace Keeper.Controls
{
  /// <summary>
  /// Interaction logic for IntervalSlidersControl.xaml
  /// </summary>
  public partial class IntervalSlidersControl : UserControl
  {
    #region DependencyProperty
    public static readonly DependencyProperty SliderIntervalProperty =
           DependencyProperty.Register("SliderInterval", typeof(Period),
           typeof(IntervalSlidersControl), new FrameworkPropertyMetadata(new Period()));

    public Period SliderInterval
    {
      get { return (Period)GetValue(SliderIntervalProperty); }
      set { SetValue(SliderIntervalProperty, value); }
    }
    #endregion

    private Point _pointDown;

    public IntervalSlidersControl()
    {
      InitializeComponent();
      Loaded += DiagramIntervalControlLoaded;
    }

    void DiagramIntervalControlLoaded(object sender, RoutedEventArgs e)
    {
//      BottomScroller.Maximum = (DateTime.Today.Year - 2002)*12 + DateTime.Today.Month;
//      BottomScroller.Value = BottomScroller.Maximum;

//      SliderInterval = DateTime.Today.GetPassedPartOfMonthWithFullThisDate();
    }

    public void Connect(int connectionId, object target)
    {
      throw new NotImplementedException();
    }

    private void ScrollBarValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      SliderInterval = new Period(new DateTime(2010,5,1), new DateTime(2010,5,31) );
    }

    private void RectangleMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      _pointDown = e.GetPosition(this);
    }

    private void RectangleMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      var pointUp = e.GetPosition(this);

      var delta = pointUp.X - _pointDown.X;
    }
  }
}
