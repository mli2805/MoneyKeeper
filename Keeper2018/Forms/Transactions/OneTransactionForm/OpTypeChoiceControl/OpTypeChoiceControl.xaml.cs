using System.Windows;
using System.Windows.Controls;

namespace Keeper2018
{
    /// <summary>
    /// Interaction logic for OpTypeChoiceControl.xaml
    /// </summary>
    public partial class OpTypeChoiceControl
    {
        public OpTypeChoiceControl()
        {
            InitializeComponent();
        }
        private void TypeButtonOnClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var viewModel = (OpTypeChoiceControlVm)button.DataContext;
            var str = (string)button.Tag;
            var number = int.Parse(str);
            viewModel.PressedButton = (OperationType)number;
        }
    }
}
