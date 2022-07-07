using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

namespace Keeper2018
{
    /// <summary>
    /// Interaction logic for GoldRatesView.xaml
    /// </summary>
    public partial class GoldRatesView
    {
        public GoldRatesView()
        {
            InitializeComponent();
        }

        private void HandleLinkClick(object sender, RoutedEventArgs e) {  
            Hyperlink hl = (Hyperlink)sender;  
            string navigateUri = hl.NavigateUri.ToString();  
            Process.Start(new ProcessStartInfo(navigateUri));  
            e.Handled = true;  
        } 
    }
}
