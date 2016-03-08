using System.Windows;
using Keeper.ViewModels.TransWithTags;
using System.Windows.Controls;

namespace Keeper.Views.TransWithTags
{
    /// <summary>
    /// Interaction logic for ExpenseTranView.xaml
    /// </summary>
    public partial class ExpenseTranView : Window
    {
        public ExpenseTranView()
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
