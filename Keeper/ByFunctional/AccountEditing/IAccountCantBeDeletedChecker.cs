using Keeper.DomainModel;

namespace Keeper.ByFunctional.AccountEditing
{
    public interface IAccountCantBeDeletedChecker
    {
        AccountCantBeDeletedReasons Check(Account account);
    }
}