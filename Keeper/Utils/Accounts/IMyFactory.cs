using Keeper.DomainModel;
using Keeper.ViewModels;

namespace Keeper.Utils.Accounts
{
  public interface IMyFactory {
    AddAndEditAccountViewModel CreateAddAndEditAccountViewModel(Account account, string title);
    Account CreateAccount();
    Account CreateAccount(Account parent);
  }
}