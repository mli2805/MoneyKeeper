using System.Windows;
using System.Windows.Markup;

namespace Keeper2018
{
    /// <summary>
    /// Interaction logic for DatePickerWithTriangles.xaml
    /// </summary>
    public partial class DatePickerWithTriangles
    {
        public DatePickerWithTriangles()
        {
            InitializeComponent();
            Language = XmlLanguage.GetLanguage("ru-RU");
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
