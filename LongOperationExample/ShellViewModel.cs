using System.IO;
using Caliburn.Micro;

namespace LongOperationExample 
{
    public class ShellViewModel : Screen, IShell
    {
      private string _statusBarItem0;
      private string _statusBarItem1;
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
        StatusBarItem0 = "Idle";
      }

      public void StartLongOperation()
      {
        StatusBarItem0 = "Long operation is started...";
        LongOperationProcess();
        StatusBarItem0 = "Long operation has ended.";
      }

      public void LongOperationProcess()
      {
        var store = File.ReadAllBytes(@"h:\t2300.sql");
        File.WriteAllBytes(@"h:\q_t2300.sql",store);
      }
    }
}