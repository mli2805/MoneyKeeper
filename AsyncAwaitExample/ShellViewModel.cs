using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;

namespace AsyncAwaitExample
{
  public class ShellViewModel : Screen, IShell
  {
    private string _statusBarItem0;
    private Visibility _isProgressBarVisible;
    private bool _isStartButtonEnabled;

    public string StatusBarItem0
    {
      get { return _statusBarItem0; }
      set
      {
        if (value == _statusBarItem0) return;
        _statusBarItem0 = value;
        NotifyOfPropertyChange(() => StatusBarItem0);
      }
    }
    public Visibility IsProgressBarVisible
    {
      get { return _isProgressBarVisible; }
      set
      {
        if (Equals(value, _isProgressBarVisible)) return;
        _isProgressBarVisible = value;
        NotifyOfPropertyChange(() => IsProgressBarVisible);
      }
    }
    public bool IsStartButtonEnabled
    {
      get { return _isStartButtonEnabled; }
      set
      {
        if (value.Equals(_isStartButtonEnabled)) return;
        _isStartButtonEnabled = value;
        NotifyOfPropertyChange(() => IsStartButtonEnabled);
      }
    }

    public ShellViewModel()
    {
      IsStartButtonEnabled = true;
      IsProgressBarVisible = Visibility.Collapsed;
      StatusBarItem0 = "Idle";
    }

    public void StartButton()
    {
      StartSeriesLongOperation();
    }

    private async void StartSeriesLongOperation()
    {
      IsStartButtonEnabled = false;
      IsProgressBarVisible = Visibility.Visible;
      StatusBarItem0 = "First long operation";

      var dd = await Task<double>.Run(() =>
      {
        double result = 0;

        for (int i = 0; i < 300000000; i++)
        {
          result += Math.Sqrt(i);
        }

        return result;
      });

      StatusBarItem0 = "Second long operation";

      var bb = await Task<bool>.Run(() =>
      {
        Thread.Sleep(5000); // Имитация длительной обработки...
        return dd > 789;
      });

      StatusBarItem0 = "Third long operation";

      if (bb)
      {
        await Task.Run(() => Thread.Sleep(5000));

      }

      StatusBarItem0 = "Idle";
      IsStartButtonEnabled = true;
      IsProgressBarVisible = Visibility.Collapsed;

    }

  }
}