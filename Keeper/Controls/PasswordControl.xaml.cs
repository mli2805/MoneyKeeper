using System;
using System.Windows;

namespace Keeper.Controls
{
  /// <summary>
  /// Interaction logic for PasswordControl.xaml
  /// </summary>
  public partial class PasswordControl
  {
    public static readonly DependencyProperty PasswordProperty =
  DependencyProperty.Register("Password", typeof(string),
                              typeof(PasswordControl), new FrameworkPropertyMetadata(""));

    public string Password
    {
      get { return (string)GetValue(PasswordProperty); }
      set { SetValue(PasswordProperty, value); }
    }


    public PasswordControl()
    {
      InitializeComponent();

      Loaded += PasswordControlLoaded;
    }

    void PasswordControlLoaded(object sender, RoutedEventArgs e)
    {
      PasswordBox.Focus();
    }

    public void Connect(int connectionId, object target)
    {
      throw new NotImplementedException();
    }

    private void LogInClick(object sender, RoutedEventArgs e)
    {
      Password = PasswordBox.Password;
    }
  }
}
