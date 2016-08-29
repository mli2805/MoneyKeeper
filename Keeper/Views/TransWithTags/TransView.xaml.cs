using System.Windows;

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
            int firstLine;
            if (MyDataGrid.SelectedIndex <= 10)
                firstLine = 0;
            else if (MyDataGrid.Items.Count - MyDataGrid.SelectedIndex > 30)
                firstLine = MyDataGrid.SelectedIndex - 10;
            else firstLine = MyDataGrid.SelectedIndex;

            MyDataGrid.ScrollIntoView(MyDataGrid.Items[firstLine]);
        }

    }
}
