using Keeper.DomainModel;

namespace Keeper.ByFunctional.EditingAccounts
{
    public interface IAccountCantBeDeletedChecker
    {
        AccountCantBeDeletedReasons Check(Account account);
    }
}