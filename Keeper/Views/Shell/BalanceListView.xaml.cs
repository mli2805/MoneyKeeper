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
  /// Interaction logic for BalanceListView.xaml
  /// </summary>
  public partial class BalanceListView : UserControl
  {
    public BalanceListView()
    {
      InitializeComponent();
    }

    public void Connect(int connectionId, object target)
    {
      throw new NotImplementedException();
    }
  }
}
