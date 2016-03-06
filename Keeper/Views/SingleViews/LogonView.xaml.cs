using System;
using System.Windows;

namespace Keeper.Views.SingleViews
{
  /// <summary>
  /// Interaction logic for LogonView.xaml
  /// </summary>
  public partial class LogonView
  {
    public string Password { get; set; }

    public LogonView()
    {
      InitializeComponent();

      Loaded += LogonViewLoaded;
    }

    void LogonViewLoaded(object sender, RoutedEventArgs e)
    {
      NowStamp.Text = string.Format("{0:dd/MM/yyyy HH:mm}", DateTime.Now);
    }

    public void Connect(int connectionId, object target)
    {
      throw new NotImplementedException();
    }

  }
}
