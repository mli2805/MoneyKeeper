namespace Keeper.ByFunctional.EditingAccounts
{
    public interface IUserInformator
    {
        void YouCannotRemoveAccountThatHasRelatedTransactions();
        void YouCannotRemoveAccountWithChildren();
        void YouCannotRemoveRootAccount();
    }
}