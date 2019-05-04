using System;
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
            if ( selector is DataGrid dataGrid && selector.SelectedItem != null && dataGrid.SelectedIndex >= 0 )
            {
                try
                {
                    dataGrid.ScrollIntoView( selector.SelectedItem );
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }
    }
}
