using System.Windows;
using System.Windows.Controls;

namespace Keeper2018
{
    /// <summary>
    /// Interaction logic for AccNameSelector.xaml
    /// </summary>
    public partial class AccNameSelector
    {
        public AccNameSelector()
        {
            InitializeComponent();
        }
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var accNameSelectorVm = (AccNameSelectorVm)DataContext;
            var buttonViewModel = (AccNameButtonVm)((Button)sender).DataContext;
            accNameSelectorVm.MyAccName = buttonViewModel.AccName;
        }
    }
}
