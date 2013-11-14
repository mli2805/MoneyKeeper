using System;
using System.Windows;

namespace Keeper.Views
{
  /// <summary>
  /// Interaction logic for LogonView.xaml
  /// </summary>
  public partial class LogonView
  {
    private string _password;
    public string Password
    {
      get { return _password; }
      set { _password = value; }
    }


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
