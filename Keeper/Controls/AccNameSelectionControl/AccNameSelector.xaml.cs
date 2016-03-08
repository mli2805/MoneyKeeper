using System.Windows;
using System.Windows.Controls;

namespace Keeper.Controls.AccNameSelectionControl
{
    /// <summary>
    /// Interaction logic for AccNameSelector.xaml
    /// </summary>
    public partial class AccNameSelector : UserControl
    {
        public AccNameSelector()
        {
            InitializeComponent();
        }
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var buttonViewModel = (AccNameButtonVm)((Button)sender).DataContext;
            buttonViewModel.Click();
        }
    }
}
