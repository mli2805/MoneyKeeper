using Keeper.DomainModel;

namespace Keeper.ByFunctional.EditingAccounts
{
    public interface IAccountLowLevelOperator
    {
        Account AddNode(Account node);
        void RemoveNode(Account node);
        void ApplyEdit(ref Account destination, Account source);
        void SortDepositAccounts(Account depositRoot);
    }
}