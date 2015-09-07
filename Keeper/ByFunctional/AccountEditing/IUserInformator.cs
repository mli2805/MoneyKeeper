namespace Keeper.ByFunctional.AccountEditing
{
    public interface IUserInformator
    {
        void YouCannotRemoveAccountThatHasRelatedTransactions();
        void YouCannotRemoveAccountWithChildren();
        void YouCannotRemoveRootAccount();
    }
}