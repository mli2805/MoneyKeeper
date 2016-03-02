using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Caliburn.Micro;
using Keeper.ByFunctional.AccountEditing;
using Keeper.DomainModel;
using Keeper.DomainModel.Transactions;
using Keeper.Views.Transactions;

namespace Keeper.ViewModels.Transactions
{
    [Export]
    class ExpenseTranViewModel : Screen, IOneTranView
    {
        private readonly KeeperDb _db;
        private readonly BalanceDuringTransactionHinter _balanceDuringTransactionHinter;
        public ListsForComboboxes ListsForComboboxes { get; set; }
        public List<Account> AccountListForExpense { get; set; }

        public TranWithTags TranInWork { get; set; }
        public string Result { get; set; }

        public string MyAccountBalance { get { return _balanceDuringTransactionHinter.GetMyAccountBalance(TranInWork); } }
        public string AmountInUsd { get { return _balanceDuringTransactionHinter.GetAmountInUsd(TranInWork); } }

        [ImportingConstructor]
        public ExpenseTranViewModel(KeeperDb db, AccountTreeStraightener accountTreeStraightener, BalanceDuringTransactionHinter balanceDuringTransactionHinter)
        {
            _db = db;
            _balanceDuringTransactionHinter = balanceDuringTransactionHinter;
            ListsForComboboxes = new ListsForComboboxes { FilterOnlyActiveAccounts = true };
            ListsForComboboxes.InitializeListsForCombobox(_db, accountTreeStraightener);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Expense transaction with tags";
        }

        public void SetTran(TranWithTags tran)
        {
            TranInWork = tran;
            AccountListForExpense = ListsForComboboxes.MyAccountsForExpense;
        }

        public void DecreaseTimestamp()
        {
            TranInWork.Timestamp = TranInWork.Timestamp.AddDays(-1);
        }

        #region горячие кнопки выбора из списков
        public void ExpenseFromMyWallet() { TranInWork.MyAccount = ListsForComboboxes.MyAccountsForShopping.FirstOrDefault(a => a.Name == "Мой кошелек"); }
        public void ExpenseFromJuliaWallet() { TranInWork.MyAccount = ListsForComboboxes.MyAccountsForShopping.FirstOrDefault(a => a.Name == "Юлин кошелек"); }
        public void ExpenseFromBibMotznaya() { TranInWork.MyAccount = ListsForComboboxes.MyAccountsForShopping.FirstOrDefault(a => a.Name == "БИБ Сберка Моцная"); }
        public void ExpenseFromBelGazSberka() { TranInWork.MyAccount = ListsForComboboxes.MyAccountsForShopping.FirstOrDefault(a => a.Name == "БГПБ Сберегательная"); }
        #endregion


        public void Save()
        {
            Result = "Save";
            TryClose();
        }

        public void Cancel()
        {
            Result = "Cancel";
            TryClose();
        }
    }
}