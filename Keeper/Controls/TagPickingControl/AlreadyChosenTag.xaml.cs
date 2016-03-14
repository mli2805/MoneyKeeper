using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Keeper.Controls.TagPickingControl
{
    /// <summary>
    /// Interaction logic for AlreadyChosenTag.xaml
    /// </summary>
    public partial class AlreadyChosenTag : UserControl
    {
        public AlreadyChosenTag()
        {
            InitializeComponent();
        }
        private void DeleteTagOnClick(object sender, RoutedEventArgs e)
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(this);

            var dc = Parent;
//            var dd = (AlreadyChosenTagVm)dc.DataContext;
        }

    }
}
