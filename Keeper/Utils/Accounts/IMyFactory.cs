using Keeper.DomainModel;
using Keeper.ViewModels;

namespace Keeper.Utils.Accounts
{
  public interface IMyFactory 
  {
    AddAndEditAccountViewModel CreateAddAndEditAccountViewModel(Account account, string windowTitle);
    OpenOrEditDepositViewModel CreateOpenOrEditDepositViewModel();
    Account CreateAccount();
    Account CloneAccount(Account account);
    Account CreateAccount(Account parent);
  }
}