using System;
using System.Windows;
using System.Windows.Controls;

namespace Keeper.Controls
{
  /// <summary>
  /// Interaction logic for ShellViewDateSelectControl.xaml
  /// </summary>
  public partial class ShellViewDateSelectControl : UserControl
  {
    public static readonly DependencyProperty ControlVisibilityProperty =
           DependencyProperty.Register("ControlVisibility", typeof(Visibility),
           typeof(ShellViewDateSelectControl), new FrameworkPropertyMetadata(new Visibility()));

    public Visibility ControlVisibility
    {
      get { return (Visibility)GetValue(ControlVisibilityProperty); }
      set { SetValue(ControlVisibilityProperty, value); }
    }

    public static readonly DependencyProperty SelectedDayProperty =
           DependencyProperty.Register("SelectedDay", typeof(DateTime),
           typeof(ShellViewDateSelectControl), new FrameworkPropertyMetadata(new DateTime()));

    public DateTime SelectedDay
    {
      get { return (DateTime)GetValue(SelectedDayProperty); }
      set { SetValue(SelectedDayProperty, value); }
    }

    public ShellViewDateSelectControl()
    {
      InitializeComponent();
    }

    public void Connect(int connectionId, object target)
    {
      throw new NotImplementedException();
    }

    private void OneDayBeforeClick(object sender, RoutedEventArgs e)
    {
      SelectedDay = SelectedDay.AddDays(-1);
    }

    private void OneMonthBeforeClick(object sender, RoutedEventArgs e)
    {
      SelectedDay = SelectedDay.AddMonths(-1);
    }

    private void OneYearBeforeClick(object sender, RoutedEventArgs e)
    {
      SelectedDay = SelectedDay.AddYears(-1);
    }

    private void OneDayAfterClick(object sender, RoutedEventArgs e)
    {
      SelectedDay = SelectedDay.AddDays(1); 
    }

    private void OneMonthAfterClick(object sender, RoutedEventArgs e)
    {
      SelectedDay = SelectedDay.AddMonths(1);
    }

    private void OneYearAfterClick(object sender, RoutedEventArgs e)
    {
      SelectedDay = SelectedDay.AddYears(1);
    }

    private void TodayClick(object sender, RoutedEventArgs e)
    {
      SelectedDay = DateTime.Today;
    }

    private void YesterdayClick(object sender, RoutedEventArgs e)
    {
      SelectedDay = DateTime.Today.AddDays(-1);
    }

    private void LastMonthEndClick(object sender, RoutedEventArgs e)
    {
      SelectedDay = DateTime.Today.AddDays(-DateTime.Today.Day + 1);
    }



  }
}
