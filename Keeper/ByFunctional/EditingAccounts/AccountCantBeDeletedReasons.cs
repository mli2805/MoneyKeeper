namespace Keeper.DomainModel
{
	public enum AccountCantBeDeletedReasons
	{
		CanDelete, IsRoot, HasChildren, HasRelatedTransactions
	}
}