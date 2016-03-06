namespace Keeper.Utils.AccountEditing
{
	public enum AccountCantBeDeletedReasons
	{
		CanDelete, IsRoot, HasChildren, HasRelatedTransactions
	}
}