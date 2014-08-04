using System.Composition;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.ViewModels;

namespace Keeper.Utils.Accounts
{
    [Export(typeof(IMyFactory))]
    public class MyFactory : IMyFactory
    {
        public AddAndEditAccountViewModel CreateAddAndEditAccountViewModel(Account account, string windowTitle)
        {
            return new AddAndEditAccountViewModel(account, windowTitle);
        }

        public Account CreateAccount()
        {
            return new Account();
        }

        public Account CloneAccount(Account account)
        {
            var result = CreateAccount();
            Account.CopyForEdit(result, account);
            return result;
        }

        public Account CreateAccount(Account parent)
        {
            return new Account() { Parent = parent };
        }



        public OpenOrEditDepositViewModel CreateOpenOrEditDepositViewModel()
        {
            return IoC.Get<OpenOrEditDepositViewModel>();
        }

    }
}