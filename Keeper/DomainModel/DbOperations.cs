using System.Collections.Generic;
using System.Composition;
using System.Linq;

namespace Keeper.DomainModel
{
	[Export]
	public sealed class AccountOperations
	{
		readonly DbIdGenerator _dbIdGenerator;

		[ImportingConstructor]
		public AccountOperations(DbIdGenerator dbIdGenerator)
		{
			_dbIdGenerator = dbIdGenerator;
		}

		public Account AddNode(Account node)
		{
			node.Id = _dbIdGenerator.Generate();
			node.Parent.Children.Add(node);
			return node;
		}

		public void RemoveNode(Account node)
		{
			node.Parent.Children.Remove(node);
		}

		public void ApplyEdit(ref Account edited, Account changesSource)
		{
			if (edited.Parent != changesSource.Parent)
			{
				changesSource.Parent.Children.Add(changesSource);
        changesSource.IsClosed = (changesSource.Parent.Is("Закрытые") || changesSource.Parent.Is("Закрытые депозиты"));
				edited.Parent.Children.Remove(edited);
			}
			Account.CopyForEdit(edited,changesSource);
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