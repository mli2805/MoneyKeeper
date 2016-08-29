namespace Keeper.Utils.AccountEditing
{
    public interface IUserInformator
    {
        void YouCannotRemoveAccountThatHasRelatedTransactions();
        void YouCannotRemoveAccountWithChildren();
        void YouCannotRemoveRootAccount();
    }
}