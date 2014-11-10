using System.Composition;
using System.Windows;

using Caliburn.Micro;

using Keeper.DomainModel;
using Keeper.DomainModel.Deposit;
using Keeper.Utils.Dialogs;

namespace Keeper.Utils.Accounts
{
	[Export(typeof(IUserEquirer))]
	public sealed class UserEquirer : IUserEquirer
	{
		const string CONFIRMATION_QUESTION =
			"Проверено, счет не используется в транзакциях.\n Удаление счета\n\n <<{0}>>\n          Удалить?";

		readonly IWindowManager _windowManager;
		readonly IAccountFactory _mAccountFactory;
		readonly IMessageBoxer _messageBoxer;

		[ImportingConstructor]
		public UserEquirer(IWindowManager windowManager, IAccountFactory accountFactory, IMessageBoxer messageBoxer)
		{
			_windowManager = windowManager;
			_mAccountFactory = accountFactory;
			_messageBoxer = messageBoxer;
		}

		public bool ToDeleteAccount(Account selectedAccount)
		{
			return _messageBoxer.Show(string.Format(CONFIRMATION_QUESTION, selectedAccount.Name), "Confirm",
			                          MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
		}
		public bool ToAddAccount(Account accountInWork)
		{
			var vm = _mAccountFactory.CreateAddAndEditAccountViewModel(accountInWork, "Добавить");
			return _windowManager.ShowDialog(vm) == true;
		}
		public bool ToEditAccount(Account accountInWork)
		{
			var vm = _mAccountFactory.CreateAddAndEditAccountViewModel(accountInWork, "Редактировать");
			return _windowManager.ShowDialog(vm) == true;
		}

        public bool ToAddDeposit(Deposit depositInWork)
        {
            var vm = _mAccountFactory.CreateOpenOrEditDepositViewModel();
            vm.InitializeForm(depositInWork, "Добавить");

            return _windowManager.ShowDialog(vm) == true;
        }
        public bool ToEditDeposit(Deposit depositInWork)
        {
            var vm = _mAccountFactory.CreateOpenOrEditDepositViewModel();
            vm.InitializeForm(depositInWork, "Редактировать");
            return _windowManager.ShowDialog(vm) == true;
        }
    }
}