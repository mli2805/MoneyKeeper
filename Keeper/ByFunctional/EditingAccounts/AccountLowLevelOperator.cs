using System.Composition;
using System.Linq;

namespace Keeper.DomainModel
{
	[Export(typeof(IAccountLowLevelOperator))]
    [Export]
	public sealed class AccountLowLevelOperator : IAccountLowLevelOperator
	{
		readonly AccountIdGenerator _accountIdGenerator;

		[ImportingConstructor]
		public AccountLowLevelOperator(AccountIdGenerator accountIdGenerator)
		{
			_accountIdGenerator = accountIdGenerator;
		}

		public Account AddNode(Account node)
		{
			node.Id = _accountIdGenerator.GenerateAccountId();
			node.Parent.Children.Add(node);
			return node;
		}

		public void RemoveNode(Account node)
		{
			node.Parent.Children.Remove(node);
		}

		public void ApplyEdit(ref Account destination, Account source)
		{
			if (destination.Parent != source.Parent)
			{
				source.Parent.Children.Add(source);
                source.IsClosed = (source.Parent.Is("Закрытые") || source.Parent.Is("Закрытые депозиты"));
				destination.Parent.Children.Remove(destination);
			  destination.Parent = source.Parent;
			}
			Account.CopyForEdit(destination,source);
		}

        public void SortDepositAccounts(Account depositRoot)
        {
            var activeDeposits = depositRoot.Children.ToList();
            depositRoot.Children.Clear();
            activeDeposits.Sort(Account.CompareAccountsByDepositFinishDate);
            foreach (var deposit in activeDeposits)
            {
                SortDepositAccounts(deposit);
                depositRoot.Children.Add(deposit);
            }
        }
	}
}