using Keeper.DomainModel;
using Keeper.ViewModels;

namespace Keeper.ByFunctional.AccountEditing
{
  public interface IAccountFactory 
  {
    AddAndEditAccountViewModel CreateAddAndEditAccountViewModel(Account account, string windowTitle);
    OpenOrEditDepositViewModel CreateOpenOrEditDepositViewModel();
    Account CreateAccount();
    Account CloneAccount(Account account);
    Account CreateAccount(Account parent);
  }
}