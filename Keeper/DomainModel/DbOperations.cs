using System.Composition;

namespace Keeper.DomainModel
{
	[Export]
	public sealed class AccountOperations
	{
		readonly DbIdGenerator mDbIdGenerator;

		[ImportingConstructor]

		public AccountOperations(DbIdGenerator dbIdGenerator)
		{
			mDbIdGenerator = dbIdGenerator;
		}

		public Account AddNode(Account node)
		{
			node.Id = mDbIdGenerator.Generate();
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
				edited.Parent.Children.Remove(edited);
			}
//			edited.Name = changesSource.Name;
			Account.CopyForEdit(edited,changesSource);
		}
	}
}