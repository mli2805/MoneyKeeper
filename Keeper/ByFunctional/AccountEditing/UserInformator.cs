using System.Composition;
using System.Windows;
using Keeper.Utils.Dialogs;

namespace Keeper.ByFunctional.AccountEditing
{
	[Export(typeof(IUserInformator))]
	public sealed class UserInformator : IUserInformator
	{
		readonly IMessageBoxer _messageBoxer;

		[ImportingConstructor]
		public UserInformator(IMessageBoxer messageBoxer)
		{
			_messageBoxer = messageBoxer;
		}

		public void YouCannotRemoveAccountThatHasRelatedTransactions()
		{
			_messageBoxer.Show("Этот счет используется в проводках!", "Отказ!", MessageBoxButton.OK, MessageBoxImage.Stop);
		}
		public void YouCannotRemoveAccountWithChildren()
		{
			_messageBoxer.Show("Удалять разрешено \n только конечные листья дерева счетов!", "Отказ!", MessageBoxButton.OK,
			                   MessageBoxImage.Stop);
		}
		public void YouCannotRemoveRootAccount()
		{
			_messageBoxer.Show("Корневой счет нельзя удалять!", "Отказ!", MessageBoxButton.OK, MessageBoxImage.Stop);
		}		
	}
}