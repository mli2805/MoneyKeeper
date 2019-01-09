using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Keeper2018
{
    /// <summary>
    /// Interaction logic for TransactionsView.xaml
    /// </summary>
    public partial class TransactionsView
    {
        public TransactionsView()
        {
            InitializeComponent();
        }

        private void OnSelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            Selector selector = sender as Selector;
            DataGrid dataGrid = selector as DataGrid;
            if ( dataGrid != null && selector.SelectedItem != null && dataGrid.SelectedIndex >= 0 )
            {
                dataGrid.ScrollIntoView( selector.SelectedItem );
            }
        }
    }
}
