namespace Keeper.ByFunctional.EditingAccounts
{
	public enum AccountCantBeDeletedReasons
	{
		CanDelete, IsRoot, HasChildren, HasRelatedTransactions
	}
}