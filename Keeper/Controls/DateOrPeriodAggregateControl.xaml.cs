using System;
using System.Windows;
using System.Windows.Controls;
using Keeper.DomainModel;

namespace Keeper.Controls
{
  /// <summary>
  /// Interaction logic for DateOrPeriodAggregateControl.xaml
  /// </summary>
  public partial class DateOrPeriodAggregateControl : UserControl
  {

    public static readonly DependencyProperty SelectedDayProperty =
       DependencyProperty.Register("SelectedDay", typeof(DateTime),
       typeof(DateOrPeriodAggregateControl), new FrameworkPropertyMetadata(new DateTime()));

    public DateTime SelectedDay
    {
      get { return (DateTime)GetValue(SelectedDayProperty); }
      set { SetValue(SelectedDayProperty, value); }
    }

    public static readonly DependencyProperty SelectedPeriodProperty =
          DependencyProperty.Register("SelectedPeriod", typeof(Period),
          typeof(DateOrPeriodAggregateControl), new FrameworkPropertyMetadata(new Period()));

    public Period SelectedPeriod
    {
      get { return (Period)GetValue(SelectedPeriodProperty); }
      set { SetValue(SelectedPeriodProperty, value); }
    }

    public static readonly DependencyProperty IsPeriodModeProperty =
      DependencyProperty.Register("IsPeriodMode", typeof(bool),
      typeof(DateOrPeriodAggregateControl), new FrameworkPropertyMetadata(new bool()));

    private Visibility _periodSelectControlVisibility;

    public bool IsPeriodMode
    {
      get
      {
        return (bool)GetValue(IsPeriodModeProperty);
      }
      set
      {
        SetValue(IsPeriodModeProperty, value);
        if (IsPeriodMode)
        {
          PeriodSelectControlVisibility = Visibility.Visible;
          DateSelectControlVisibility = Visibility.Collapsed;
        }
        else
        {
          PeriodSelectControlVisibility = Visibility.Collapsed;
          DateSelectControlVisibility = Visibility.Visible;
        }
      }
    }

    public Visibility PeriodSelectControlVisibility
    {
      get { return _periodSelectControlVisibility; }
      set { _periodSelectControlVisibility = value; 
     }
    }

    public Visibility DateSelectControlVisibility { get; set; }
    public DateTime TranslatedDate { get; set; }
    public Period TranslatedPeriod { get; set; }

    public DateOrPeriodAggregateControl()
    {
      InitializeComponent();

      PeriodSelectControlVisibility = Visibility.Collapsed;
      DateSelectControlVisibility = Visibility.Visible;

    }

    public void Connect(int connectionId, object target)
    {
      throw new NotImplementedException();
    }
  }
}
