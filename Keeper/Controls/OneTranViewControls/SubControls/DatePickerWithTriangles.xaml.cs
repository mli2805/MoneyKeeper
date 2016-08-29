using System.Windows;
using System.Windows.Controls;

namespace Keeper.Controls.OneTranViewControls.SubControls
{
    /// <summary>
    /// Interaction logic for DatePickerWithTriangles.xaml
    /// </summary>
    public partial class DatePickerWithTriangles : UserControl
    {
        public DatePickerWithTriangles()
        {
            InitializeComponent();
        }


        private void ButtonDecreaseOnClick(object sender, RoutedEventArgs e)
        {
            var dc = (DatePickerWithTrianglesVm) DataContext;
            dc.SelectedDate = dc.SelectedDate.AddDays(-1);
        }

        private void ButtonIncreaseOnClick(object sender, RoutedEventArgs e)
        {
            var dc = (DatePickerWithTrianglesVm)DataContext;
            dc.SelectedDate = dc.SelectedDate.AddDays(1);
        }
    }
}
