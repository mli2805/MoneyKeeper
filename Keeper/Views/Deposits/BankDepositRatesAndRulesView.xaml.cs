using System;
using System.Windows;

namespace Keeper.Views.Deposits
{
  /// <summary>
  /// Interaction logic for BankDepositRatesAndRulesView.xaml
  /// </summary>
  public partial class BankDepositRatesAndRulesView : Window
  {
    public BankDepositRatesAndRulesView()
    {
      InitializeComponent();
    }

    public void Connect(int connectionId, object target)
    {
      throw new NotImplementedException();
    }

    private void HasAdditionalProcent_Click(object sender, RoutedEventArgs e)
    {
        AdditionalProcent.Visibility = HasAdditionalProcent.IsChecked != true ? Visibility.Hidden : Visibility.Visible;

    }

    private void Window_Activated(object sender, EventArgs e)
    {
        AdditionalProcent.Visibility = HasAdditionalProcent.IsChecked != true ? Visibility.Hidden : Visibility.Visible;

    }
  }
}
