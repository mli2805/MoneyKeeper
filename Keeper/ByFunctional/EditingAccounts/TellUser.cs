using System.Composition;
using System.Windows;

using Keeper.Utils.Dialogs;

namespace Keeper.Utils.Accounts
{
	[Export]
	public sealed class TellUser
	{
		readonly IMessageBoxer mMessageBoxer;

		[ImportingConstructor]
		public TellUser(IMessageBoxer messageBoxer)
		{
			mMessageBoxer = messageBoxer;
		}

		public void YouCannotRemoveAccountThatHasRelatedTransactions()
		{
			mMessageBoxer.Show("Этот счет используется в проводках!", "Отказ!", MessageBoxButton.OK, MessageBoxImage.Stop);
		}
		public void YouCannotRemoveAccountWithChildren()
		{
			mMessageBoxer.Show("Удалять разрешено \n только конечные листья дерева счетов!", "Отказ!", MessageBoxButton.OK,
			                   MessageBoxImage.Stop);
		}
		public void YouCannotRemoveRootAccount()
		{
			mMessageBoxer.Show("Корневой счет нельзя удалять!", "Отказ!", MessageBoxButton.OK, MessageBoxImage.Stop);
		}		
	}
}