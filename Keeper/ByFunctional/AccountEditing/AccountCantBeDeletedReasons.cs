namespace Keeper.ByFunctional.AccountEditing
{
	public enum AccountCantBeDeletedReasons
	{
		CanDelete, IsRoot, HasChildren, HasRelatedTransactions
	}
}