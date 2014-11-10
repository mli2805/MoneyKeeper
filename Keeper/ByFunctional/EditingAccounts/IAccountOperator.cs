namespace Keeper.DomainModel
{
    public interface IAccountOperator
    {
        Account AddNode(Account node);
        void RemoveNode(Account node);
        void ApplyEdit(ref Account destination, Account source);
        void SortDepositAccounts(Account depositRoot);
    }
}