using System.Windows;
using Keeper.ViewModels.TransWithTags;

namespace Keeper.Views.TransWithTags
{
    /// <summary>
    /// Interaction logic for TransView.xaml
    /// </summary>
    public partial class TransView : Window
    {
        public TransView()
        {
            InitializeComponent();

        }

        private void MyDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int firstLine = MyDataGrid.SelectedIndex > 10 ? MyDataGrid.SelectedIndex - 10 : 0;
            MyDataGrid.ScrollIntoView(MyDataGrid.Items[firstLine]);
        }
    }
}
