using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Keeper.Views.Shell
{
  /// <summary>
  /// Interaction logic for TwoSelectorsView.xaml
  /// </summary>
  public partial class TwoSelectorsView : UserControl
  {
    public TwoSelectorsView()
    {
      InitializeComponent();
    }

    public void Connect(int connectionId, object target)
    {
      throw new NotImplementedException();
    }

    private void ShellViewPeriodSelectControl_Loaded_1(object sender, RoutedEventArgs e)
    {

    }
  }
}
