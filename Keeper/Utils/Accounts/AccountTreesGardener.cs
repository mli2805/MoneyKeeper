using System.Composition;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils.Dialogs;
using Keeper.ViewModels;

namespace Keeper.Utils.Accounts
{
	[Export]
	public class AccountTreesGardener
	{
    const string ROOT_ACCOUNT_COULDNT_BE_REMOVED = "Корневой счет нельзя удалять!";
	  const string ONLY_LEAVES_COULD_BE_REMOVED = "Удалять разрешено \n только конечные листья дерева счетов!";
	  const string ACCOUNT_USED_IN_TRANSACTIONS_COULDNT_BE_REMOVED = "Этот счет используется в проводках!";
    public const string CONFIRMATION_QUESTION =
	    "Проверено, счет не используется в транзакциях.\n Удаление счета\n\n <<{0}>>\n          Удалить?";

		readonly IWindowManager mWindowManager;
		readonly IMyFactory mMyFactory;
	  private readonly IMessageBoxer _messageBoxer;
	  private readonly KeeperDb _db;
		readonly AccountTreeStraightener mAccountTreeStraightener;

		[ImportingConstructor]
		public AccountTreesGardener(KeeperDb db, AccountTreeStraightener accountTreeStraightener,
			IWindowManager windowManager, IMyFactory myFactory, IMessageBoxer messageBoxer)
		{
			mWindowManager = windowManager;
			mMyFactory = myFactory;
		  _messageBoxer = messageBoxer;
		  _db = db;
			mAccountTreeStraightener = accountTreeStraightener;
		}

		public void RemoveAccount(Account selectedAccount)
		{
			if (selectedAccount.Parent == null)
			{
        _messageBoxer.Show("Корневой счет нельзя удалять!", "Отказ!",MessageBoxButton.OK, MessageBoxImage.Stop);
				return;
			}
			if (selectedAccount.Children.Count > 0)
			{
        _messageBoxer.Show("Удалять разрешено \n только конечные листья дерева счетов!", "Отказ!",MessageBoxButton.OK, MessageBoxImage.Stop);
				return;
			}
			// такой запрос возвращает не коллекцию, а энумератор
			var tr = from transaction in _db.Transactions
					 where transaction.Debet == selectedAccount || transaction.Credit == selectedAccount || transaction.Article == selectedAccount
					 select transaction;

			// Any() пытается двинуться по этому энумератору и если может, то true
			if (tr.Any())
			{
        _messageBoxer.Show("Этот счет используется в проводках!", "Отказ!",MessageBoxButton.OK, MessageBoxImage.Stop);
				return;
			}
      if (_messageBoxer.Show(string.Format(CONFIRMATION_QUESTION, selectedAccount.Name), "Confirm",
								MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) 
			                                             selectedAccount.Parent.Children.Remove(selectedAccount);
		}

    // если при добавлении поменять родителя - проблемы
		public Account AddAccount(Account selectedAccount)
		{
			var accountInWork = mMyFactory.CreateAccount(selectedAccount);
			var vm = mMyFactory.CreateAddAndEditAccountViewModel(accountInWork, "Добавить");
			if (mWindowManager.ShowDialog(vm) != true) return null;

			// Если во время тестирования не получится написать такой тест который становится красным,
			// если удалить эту строчку, то она не нужна!
//			selectedAccount = accountInWork.Parent;

			accountInWork.Id = (from account in mAccountTreeStraightener.Flatten(_db.Accounts) select account.Id).Max() + 1;
			selectedAccount.Children.Add(accountInWork);
		  return accountInWork;
		}

		public void ChangeAccount(Account selectedAccount)
		{
		  var accountInWork = mMyFactory.CreateAccount();
			Account.CopyForEdit(accountInWork, selectedAccount);
      var vm = mMyFactory.CreateAddAndEditAccountViewModel(accountInWork, "Редактировать");
      if (mWindowManager.ShowDialog(vm) != true) return;

			if (selectedAccount.Parent != accountInWork.Parent)
			{
				accountInWork.Parent.Children.Add(accountInWork);
				selectedAccount.Parent.Children.Remove(selectedAccount);
			}
			selectedAccount.Name = accountInWork.Name;
		}

	}
	public interface IMyFactory {
		AddAndEditAccountViewModel CreateAddAndEditAccountViewModel(Account account, string title);
		Account CreateAccount();
		Account CreateAccount(Account parent);
	}

	[Export (typeof(IMyFactory))]
	public class MyFactory : IMyFactory
	{
		public AddAndEditAccountViewModel CreateAddAndEditAccountViewModel(Account account, string title)
		{
			return new AddAndEditAccountViewModel(account, title);
		}

		public Account CreateAccount()
		{
			return new Account();
		}

		public Account CreateAccount(Account parent)
		{
			return new Account(){Parent = parent};
		}

	}
}
