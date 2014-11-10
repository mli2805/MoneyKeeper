namespace Keeper.Utils.Accounts
{
    public interface IUserInformator
    {
        void YouCannotRemoveAccountThatHasRelatedTransactions();
        void YouCannotRemoveAccountWithChildren();
        void YouCannotRemoveRootAccount();
    }
}