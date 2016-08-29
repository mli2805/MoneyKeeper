using System.Windows.Media;
using Caliburn.Micro;

namespace Keeper.ViewModels.SingleViews
{
  class LogonViewModel : Screen
  {
    private string _password;
    private Brush _propositionColor;
    public string Password
    {
      get { return _password; }
      set
      {
        _password = value;
        LogIn();
      }
    }
    public Brush PropositionColor
    {
      get { return _propositionColor; }
      set
      {
        if (Equals(value, _propositionColor)) return;
        _propositionColor = value;
        NotifyOfPropertyChange(() => PropositionColor);
      }
    }
    private readonly string _correctPassword;

    public bool Result { get; set; }

    public LogonViewModel(string correctPassword)
    {
      _correctPassword = correctPassword;
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = "Logon";
      PropositionColor = Brushes.Blue;
    }

    public void LogIn()
    {
      Result = CheckPassword(Password);
      if (Result) TryClose();
//      else
//        PropositionColor = Equals(PropositionColor, Brushes.Red) ? Brushes.DarkMagenta : Brushes.Red;
    }

    public void Escape()
    {
      Result = false;
      TryClose();
    }

    public bool CheckPassword(string password)
    {
      return password == _correctPassword;
    }
  }
}
