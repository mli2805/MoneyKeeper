using System.Windows;

namespace Keeper.Views
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
    
  }
}
