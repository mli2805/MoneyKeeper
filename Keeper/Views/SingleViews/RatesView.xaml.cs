using System;
using System.Windows;

namespace Keeper.Views.SingleViews
{
  /// <summary>
  /// Interaction logic for RatesView.xaml
  /// </summary>
  public partial class RatesView : Window
  {
    public RatesView()
    {
      InitializeComponent();

      InputRatesExpander.Expanded += InputRatesExpanderExpanded;
    }

    void InputRatesExpanderExpanded(object sender, RoutedEventArgs e)
    {
      RatesGrid.ScrollIntoView(RatesGrid.Items[RatesGrid.Items.Count-1]);
    }

    public void Connect(int connectionId, object target)
    {
      throw new NotImplementedException();
    }

//    protected override void OnActivated(System.EventArgs e)
//    {
//      base.OnActivated(e);
//      RatesGrid.ScrollIntoView(RatesGrid.Items[RatesGrid.Items.Count - 1]);
//    }
  }
}
