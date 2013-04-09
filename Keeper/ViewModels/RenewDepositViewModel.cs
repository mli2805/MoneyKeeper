using System;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  class RenewDepositViewModel : Screen
  {
    public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    private readonly Deposit _oldDeposit;

    public string OldDepositName { get; set; }
    public string DepositCurrency { get; set; }
    public Account BankAccount { get; set; }
    public decimal Procents { get; set; }
    public string NewDepositName { get; set; }

    public RenewDepositViewModel(Deposit oldDeposit)
    {
      _oldDeposit = oldDeposit;

      OldDepositName = _oldDeposit.Account.Name;
      DepositCurrency = _oldDeposit.MainCurrency.ToString().ToLower();
      BankAccount = FindBankAccount();
      Procents = _oldDeposit.Forecast;
      NewDepositName = BuildNewName();
    }

    private Account FindBankAccount()
    {
      var st = OldDepositName.Substring(0, OldDepositName.IndexOf(' '));
      return Db.FindAccountInTree(st);
    }

    private string BuildNewName()
    {
      var st = OldDepositName.Substring(0, OldDepositName.IndexOf('/') - 2).Trim();
      DateTime newFinish = _oldDeposit.Finish + (_oldDeposit.Finish - _oldDeposit.Start);
      string period = String.Format("{0:d/MM/yyyy} - {1:d/MM/yyyy}", _oldDeposit.Finish, newFinish).Replace('.', '/');
      return String.Format("{0} {1} {2}%", st, period, _oldDeposit.DepositRate);
    }

    public void Accept()
    {
      
    }

    public void Decline()
    {
      
    }

  }
}
