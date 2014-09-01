namespace Keeper.DomainModel
{
	public enum DeleteReasons
	{
		CanDelete, IsRoot, HasChildren, HasRelatedTransactions
	}
}