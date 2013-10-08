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

namespace Keeper.Views.DiagramViews
{
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>
  public partial class UserControl1 : UserControl
  {
    public static readonly DependencyProperty CurrentTimeProperty =
      DependencyProperty.Register("CurrentTime", typeof(DateTime),
                                  typeof(UserControl1), new FrameworkPropertyMetadata(DateTime.Now));

    public DateTime CurrentTime
    {
      get { return (DateTime)GetValue(CurrentTimeProperty); }
      set { SetValue(CurrentTimeProperty, value); }
    }


    public UserControl1()
    {
      InitializeComponent();
    }
  }
}
