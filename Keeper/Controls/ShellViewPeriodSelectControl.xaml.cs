using System;
using System.Windows;
using System.Windows.Controls;
using Keeper.Utils;

namespace Keeper.Controls
{
  /// <summary>
  /// Interaction logic for ShellViewPeriodSelectControl.xaml
  /// </summary>
  public partial class ShellViewPeriodSelectControl : UserControl
  {
    public ShellViewPeriodSelectControl()
    {
      InitializeComponent();
    }

    public void Connect(int connectionId, object target)
    {
      throw new NotImplementedException();
    }

    public static readonly DependencyProperty ControlVisibilityProperty =
       DependencyProperty.Register("ControlVisibility", typeof(Visibility),
       typeof(ShellViewDateSelectControl), new FrameworkPropertyMetadata(new Visibility()));

    public Visibility ControlVisibility
    {
      get { return (Visibility)GetValue(ControlVisibilityProperty); }
      set { SetValue(ControlVisibilityProperty, value); }
    }

    public static readonly DependencyProperty SelectedPeriodProperty =
           DependencyProperty.Register("SelectedPeriod", typeof(Period),
           typeof(ShellViewDateSelectControl), new FrameworkPropertyMetadata(new Period()));

    public Period SelectedPeriod
    {
      get { return (Period)GetValue(SelectedPeriodProperty); }
      set { SetValue(SelectedPeriodProperty, value); }
    }


  }
}
