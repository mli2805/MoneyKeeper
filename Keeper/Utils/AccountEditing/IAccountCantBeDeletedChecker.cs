using Keeper.DomainModel.DbTypes;

namespace Keeper.Utils.AccountEditing
{
    public interface IAccountCantBeDeletedChecker
    {
        AccountCantBeDeletedReasons Check(Account account);
    }
}