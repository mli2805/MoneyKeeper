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
using System.Windows.Shapes;

namespace Keeper.Views
{
  /// <summary>
  /// Interaction logic for RatesView.xaml
  /// </summary>
  public partial class RatesView : Window
  {
    public RatesView()
    {
      InitializeComponent();
      Loaded +=OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
      

    }
  }
}
