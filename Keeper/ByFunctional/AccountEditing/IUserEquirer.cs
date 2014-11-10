using Keeper.DomainModel;
using Keeper.DomainModel.Deposit;

namespace Keeper.ByFunctional.AccountEditing
{
    public interface IUserEquirer
    {
        bool ToDeleteAccount(Account selectedAccount);
        bool ToAddAccount(Account accountInWork);
        bool ToEditAccount(Account accountInWork);
        bool ToAddDeposit(Deposit depositInWork);
        bool ToEditDeposit(Deposit depositInWork);
    }
}