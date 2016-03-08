using System.Windows;
using System.Windows.Controls;
using Keeper.ViewModels.TransWithTags;

namespace Keeper.Controls
{
    /// <summary>
    /// Interaction logic for TestControl.xaml
    /// </summary>
    public partial class TestControl : UserControl
    {
        public TestControl()
        {
            InitializeComponent();
        }
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var buttonViewModel = (ButtonViewModel)((Button)sender).DataContext;
            buttonViewModel.Click();
        }
    }
}
