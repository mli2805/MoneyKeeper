using System;
using System.Windows;
using System.Windows.Controls;
using Keeper.DomainModel;

namespace Keeper.Controls
{
  /// <summary>
  /// Interaction logic for IntervalExpositionControl.xaml
  /// </summary>
  public partial class IntervalExpositionControl : UserControl
  {

    #region DependencyProperty
    public static readonly DependencyProperty ExpositionIntervalProperty =
           DependencyProperty.Register("ExpositionInterval", typeof(Period),
           typeof(IntervalExpositionControl), new FrameworkPropertyMetadata(new Period()));

    public Period ExpositionInterval
    {
      get { return (Period)GetValue(ExpositionIntervalProperty); }
      set
      {
        SetValue(ExpositionIntervalProperty, value);
        BuildExposition();
      }
    }

    public static readonly DependencyProperty ExpositionModeProperty =
      DependencyProperty.Register("ExpositionMode", typeof(int),
                                  typeof(IntervalExpositionControl), new FrameworkPropertyMetadata(2));

    public int ExpositionMode
    {
      get { return (int)GetValue(ExpositionModeProperty); }
      set
      {
        SetValue(ExpositionModeProperty, value);
        BuildExposition();
      }
    }
    #endregion

    public IntervalExpositionControl()
    {
      InitializeComponent();
      Loaded += IntervalExpositionControlLoaded;
    }

    void IntervalExpositionControlLoaded(object sender, RoutedEventArgs e)
    {
      Exposition.Text = "Стартуем!";
    }

    public void Connect(int connectionId, object target)
    {
      throw new NotImplementedException();
    }

    private void BuildExposition()
    {
      if (ExpositionMode == 2) // месяца
      {
        if (ExpositionInterval.Start.Year == ExpositionInterval.Finish.Year && ExpositionInterval.Start.Month == ExpositionInterval.Finish.Month)
          Exposition.Text = string.Format("   {0:MMMM yyyy}", new DateTime(ExpositionInterval.Start.Year, ExpositionInterval.Start.Month, 1));
        else
          Exposition.Text = string.Format("   {0:MMMM yyyy}  {1:MMMM yyyy}",
                                    new DateTime(ExpositionInterval.Start.Year, ExpositionInterval.Start.Month, 1),
                                    new DateTime(ExpositionInterval.Finish.Year, ExpositionInterval.Finish.Month, 1));
      }
      if (ExpositionMode == 1) // годы
      {
        if (ExpositionInterval.Start.Year == ExpositionInterval.Finish.Year)
          Exposition.Text = string.Format("   {0} год", ExpositionInterval.Start.Year);
        else
          Exposition.Text = string.Format("   {0} - {1} годы", ExpositionInterval.Start.Year, ExpositionInterval.Finish.Year);
      }
    }
  }
}
