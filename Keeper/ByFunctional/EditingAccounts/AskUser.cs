using System.Composition;
using System.Windows;

using Caliburn.Micro;

using Keeper.DomainModel;
using Keeper.Utils.Dialogs;

namespace Keeper.Utils.Accounts
{
	[Export]
	public sealed class AskUser
	{
		const string CONFIRMATION_QUESTION =
			"Проверено, счет не используется в транзакциях.\n Удаление счета\n\n <<{0}>>\n          Удалить?";

		readonly IWindowManager mWindowManager;
		readonly IMyFactory mMyFactory;
		readonly IMessageBoxer mMessageBoxer;

		[ImportingConstructor]
		public AskUser(IWindowManager windowManager, IMyFactory myFactory, IMessageBoxer messageBoxer)
		{
			mWindowManager = windowManager;
			mMyFactory = myFactory;
			mMessageBoxer = messageBoxer;
		}

		public bool ToDeleteAccount(Account selectedAccount)
		{
			return mMessageBoxer.Show(string.Format(CONFIRMATION_QUESTION, selectedAccount.Name), "Confirm",
			                          MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
		}
		public bool ToAddAccount(Account accountInWork)
		{
			var vm = mMyFactory.CreateAddAndEditAccountViewModel(accountInWork, "Добавить");
			if (mWindowManager.ShowDialog(vm) != true) return false;
			return true;
		}
		public bool ToEditAccount(Account accountInWork)
		{
			var vm = mMyFactory.CreateAddAndEditAccountViewModel(accountInWork, "Редактировать");
			if (mWindowManager.ShowDialog(vm) != true) return false;
			return true;
		}

    public bool ToAddDeposit(Deposit depositInWork)
    {
      var vm = mMyFactory.CreateOpenOrEditDepositViewModel();
      vm.InitializeForm(depositInWork, "Добавить");
      
      if (mWindowManager.ShowDialog(vm) != true) return false;
      return true;
    }
    public bool ToEditDeposit(Deposit depositInWork)
    {
      var vm = mMyFactory.CreateOpenOrEditDepositViewModel();
      vm.InitializeForm(depositInWork, "Редактировать");
      if (mWindowManager.ShowDialog(vm) != true) return false;
      return true;
    }
  }
}