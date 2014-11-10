namespace Keeper.DomainModel
{
    public interface IAccountCantBeDeletedChecker
    {
        AccountCantBeDeletedReasons Check(Account account);
    }
}