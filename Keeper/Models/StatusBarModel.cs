using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;

namespace Keeper.Models
{
  [Export]
  public class StatusBarModel : PropertyChangedBase
  {
    private string _item0;
    private Visibility _progressBarVisibility;
    private string _message;

    public string Item0
    {
      get { return _item0; }
      set
      {
        if (value == _item0) return;
        _item0 = value;
        NotifyOfPropertyChange(() => Item0);
      }
    }

    public Visibility ProgressBarVisibility
    {
      get { return _progressBarVisibility; }
      set
      {
        if (Equals(value, _progressBarVisibility)) return;
        _progressBarVisibility = value;
        NotifyOfPropertyChange(() => ProgressBarVisibility);
      }
    }

    public string Message
    {
      get { return _message; }
      set
      {
        if (value == _message) return;
        _message = value;
        NotifyOfPropertyChange(() => Message);
      }
    }
  }
}
