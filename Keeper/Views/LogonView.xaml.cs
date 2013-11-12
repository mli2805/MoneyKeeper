﻿using System;
using System.Windows;

namespace Keeper.Views
{
  /// <summary>
  /// Interaction logic for LogonView.xaml
  /// </summary>
  public partial class LogonView
  {
    public LogonView()
    {
      InitializeComponent();

      Loaded += LogonViewLoaded;
    }

    void LogonViewLoaded(object sender, RoutedEventArgs e)
    {
      NowStamp.Text = string.Format("{0:HH:mm dd/MM/yyyy}",DateTime.Now);
    }

    public void Connect(int connectionId, object target)
    {
      throw new NotImplementedException();
    }
  }
}
