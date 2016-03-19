using System.Windows;
using System.Windows.Controls;
using Keeper.DomainModel.Enumes;

namespace Keeper.Controls
{
    /// <summary>
    /// Interaction logic for OpTypeChoiceControl.xaml
    /// </summary>
    public partial class OpTypeChoiceControl : UserControl
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
