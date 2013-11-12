using Caliburn.Micro;

namespace Keeper.ViewModels
{
  class LogonViewModel : Screen
  {
    public string Password { get; set; }

    public void LogIn()
    {
      if (CheckPassword(Password)) TryClose();
    }

    public bool CheckPassword(string password)
    {
      return true;
    }
  }
}
