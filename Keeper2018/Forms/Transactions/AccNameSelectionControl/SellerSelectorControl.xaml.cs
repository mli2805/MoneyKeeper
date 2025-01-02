using System.Windows;
using System.Windows.Controls;

namespace Keeper2018
{
    /// <summary>
    /// Interaction logic for SellerSelectorControl.xaml
    /// </summary>
    public partial class SellerSelectorControl
    {
        public SellerSelectorControl()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var myControl = (SellerSelectorVm)DataContext;
            var buttonViewModel = (AccNameButtonVm)((Button)sender).DataContext;
            myControl.MyAccName = buttonViewModel.AccName;
        }
    }
}
