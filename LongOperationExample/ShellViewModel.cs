using System.ComponentModel;
using System.IO;
using System.Windows;
using Caliburn.Micro;

namespace LongOperationExample 
{
    public class ShellViewModel : Screen, IShell
    {
      private readonly BackgroundWorker _backgroundWorker;
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


      private string _statusBarItem0;
      private string _statusBarItem1;
      private Visibility _isProgressBarVisible;

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
      public string StatusBarItem1
      {
        get { return _statusBarItem1; }
        set
        {
          if (value == _statusBarItem1) return;
          _statusBarItem1 = value;
          NotifyOfPropertyChange(() => StatusBarItem1);
        }
      }


      public ShellViewModel()
      {
        _backgroundWorker = new BackgroundWorker();
        _backgroundWorker.WorkerReportsProgress = true;
        _backgroundWorker.WorkerSupportsCancellation = true;
        _backgroundWorker.DoWork += BackgroundWorkerDoWork;
        _backgroundWorker.ProgressChanged += _backgroundWorker_ProgressChanged;
        _backgroundWorker.RunWorkerCompleted += BackgroundWorkerRunWorkerCompleted;

        StatusBarItem0 = "Idle";
        IsProgressBarVisible = Visibility.Collapsed;
      }

      void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
      {
        LongOperationProcess();
      }

      void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
      {
        throw new System.NotImplementedException();
      }

      void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
      {
        StatusBarItem0 = "Long operation has ended.";
        IsProgressBarVisible = Visibility.Collapsed;
      }


      public void StartLongOperation()
      {
        StatusBarItem0 = "Long operation is started...";
        IsProgressBarVisible = Visibility.Visible;
        _backgroundWorker.RunWorkerAsync(); 
      }

      public void LongOperationProcess()
      {
        var store = File.ReadAllBytes(@"h:\t2300.sql");
        File.WriteAllBytes(@"h:\q_t2300.sql",store);
      }
    }
}