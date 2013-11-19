using System;
using System.Windows;
using System.Windows.Controls;
using Keeper.Utils;

namespace Keeper.Controls
{
  /// <summary>
  /// Interaction logic for ShellViewPeriodSelectControl.xaml
  /// </summary>
  public partial class ShellViewPeriodSelectControl
  {
    public static readonly DependencyProperty SelectedPeriodProperty =
           DependencyProperty.Register("SelectedPeriod", typeof(Period),
           typeof(ShellViewPeriodSelectControl), new FrameworkPropertyMetadata(new Period()));

    public Period SelectedPeriod
    {
      get { return (Period)GetValue(SelectedPeriodProperty); }
      set
      {
        if (SelectedPeriod.Equals(value)) return;
        SetValue(SelectedPeriodProperty, value);
        StartDatePicker.SelectedDate = value.GetStart();
        FinishDatePicker.SelectedDate = value.GetFinish();
      }
    }

    public ShellViewPeriodSelectControl()
    {
      InitializeComponent();

      Loaded += ShellViewPeriodSelectControlLoaded;
    }

    void ShellViewPeriodSelectControlLoaded(object sender, RoutedEventArgs e)
    {
      SelectedPeriod = new Period(new DayProcessor(DateTime.Today).BeforeThisDay(),
        new DayProcessor(DateTime.Today).AfterThisDay());
    }

    public void Connect(int connectionId, object target)
    {
      throw new NotImplementedException();
    }

    private void OneDayBeforeClick(object sender, RoutedEventArgs e) { SelectedPeriod = new Period(SelectedPeriod.GetStart().AddDays(-1), SelectedPeriod.GetFinish().AddDays(-1)); }

    private void OneMonthBeforeClick(object sender, RoutedEventArgs e)
    {

      var finish = SelectedPeriod.GetFinish();
      finish = IsLastDayOfMonth(finish) ? finish.AddDays(-finish.Day) : finish.AddMonths(-1);
      SelectedPeriod = new Period(SelectedPeriod.GetStart().AddMonths(-1), finish);
    }

    private void OneYearBeforeClick(object sender, RoutedEventArgs e) { SelectedPeriod = new Period(SelectedPeriod.GetStart().AddYears(-1), SelectedPeriod.GetFinish().AddYears(-1)); }
    private void OneDayAfterClick(object sender, RoutedEventArgs e) { SelectedPeriod = new Period(SelectedPeriod.GetStart().AddDays(1), SelectedPeriod.GetFinish().AddDays(1)); }
    private void OneMonthAfterClick(object sender, RoutedEventArgs e)
    {
      DateTime finish;
      if (IsLastDayOfMonth(SelectedPeriod.GetFinish()))
      {
        finish = SelectedPeriod.GetFinish().AddMonths(2);
        finish = finish.AddDays(-finish.Day);
      }
      else finish = SelectedPeriod.GetFinish().AddMonths(1);

      SelectedPeriod = new Period(SelectedPeriod.GetStart().AddMonths(1), finish);
    }
    private void OneYearAfterClick(object sender, RoutedEventArgs e) { SelectedPeriod = new Period(SelectedPeriod.GetStart().AddYears(1), SelectedPeriod.GetFinish().AddYears(1)); }
    private void TodayClick(object sender, RoutedEventArgs e) { SelectedPeriod = new Period(DateTime.Today, DateTime.Today); }
    private void YesterdayClick(object sender, RoutedEventArgs e) { SelectedPeriod = new Period(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(-1)); }
    private void ThisMonthClick(object sender, RoutedEventArgs e) { SelectedPeriod = new Period(DateTime.Today.AddDays(-DateTime.Today.Day + 1), DateTime.Today); }
    private void LastMonthClick(object sender, RoutedEventArgs e)
    {
      var finish = DateTime.Today.AddDays(-DateTime.Today.Day);
      SelectedPeriod = new Period(finish.AddDays(-finish.Day + 1), finish);
    }
    private void ThisYearPaymentsClick(object sender, RoutedEventArgs e) { SelectedPeriod = new Period(DateTime.Today.AddDays(-DateTime.Today.DayOfYear + 1), DateTime.Today); }
    private void LastYearPaymentsClick(object sender, RoutedEventArgs e)
    {
      var finish = new DayProcessor(DateTime.Today.AddDays(-DateTime.Today.DayOfYear)).AfterThisDay();
      SelectedPeriod = new Period(finish.AddDays(-finish.DayOfYear + 1), finish);
    }

    private bool IsLastDayOfMonth(DateTime date) { return date.Month != date.AddDays(1).Month; }

    private void StartDatePickerSelectedDateChanged(object sender, SelectionChangedEventArgs e)
    {
      if (StartDatePicker.SelectedDate == null) StartDatePicker.SelectedDate = SelectedPeriod.GetStart();
      else
        SelectedPeriod = new Period(new DayProcessor((DateTime)StartDatePicker.SelectedDate).BeforeThisDay(), SelectedPeriod.GetFinish());
    }

    private void FinishDatePickerSelectedDateChanged(object sender, SelectionChangedEventArgs e)
    {
      if (FinishDatePicker.SelectedDate == null) FinishDatePicker.SelectedDate = SelectedPeriod.GetFinish(); 
      else
        SelectedPeriod = new Period(SelectedPeriod.GetStart(), new DayProcessor((DateTime)FinishDatePicker.SelectedDate).AfterThisDay());
    }

  }
}
