using System.Windows;
using System.Windows.Controls;

namespace Keeper.Views.Transactions
{
    /// <summary>
    /// Interaction logic for TrView.xaml
    /// </summary>
    public partial class TrView : Window
    {
        public TrView()
        {
            InitializeComponent();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((DataGrid)sender).UnselectAllCells();
        }
    }
}
