using Caliburn.Micro;

namespace Keeper.ViewModels
{
  class LogonViewModel : Screen
  {
    private string _password;
    public string Password
    {
      get { return _password; }
      set
      {
        _password = value;
        LogIn();
      }
    }

    public bool Result { get; set; }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = "Logon";
    }

    public void LogIn()
    {
      Result = CheckPassword(Password);
      if (Result) TryClose();
    }

    public void Escape()
    {
      Result = false;
      TryClose();
    }

    public bool CheckPassword(string password)
    {
      return password == "123";
    }
  }
}
