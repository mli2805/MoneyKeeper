using System.Windows;
using System.Windows.Controls;

namespace TextBoxStyle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public decimal Amount { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            Amount = 3.04m;
            MyListBox.Items.Add("Hello!");
            MyListBox.Items.Add("Amount Text Box should contain number 3");

        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SwitchStyle_Click(object sender, RoutedEventArgs e)
        {
            var template = (ControlTemplate)FindResource("MyControlTemplateWithFractional");
            AmounTextBox.Template = template;
        }
    }
}
