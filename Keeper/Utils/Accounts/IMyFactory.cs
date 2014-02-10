using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.ViewModels;

namespace Keeper.Utils.Accounts
{
  public interface IMyFactory {
    AddAndEditAccountViewModel CreateAddAndEditAccountViewModel(Account account, string windowTitle);
    OpenOrEditDepositViewModel CreateOpenOrEditDepositViewModel(Deposit deposit, string windowTitle);
    Account CreateAccount();
    Account CloneAccount(Account account);
    Account CreateAccount(Account parent);
  }
}