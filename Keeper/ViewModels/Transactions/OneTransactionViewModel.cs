using System.Collections.Generic;
using System.Composition;
using Caliburn.Micro;
using Keeper.ByFunctional.AccountEditing;
using Keeper.DomainModel;
using Keeper.DomainModel.Transactions;

namespace Keeper.ViewModels.Transactions
{
    [Export]
    class OneTransactionViewModel : Screen
    {
        private readonly KeeperDb _db;
        private readonly BalanceDuringTransactionHinter _balanceDuringTransactionHinter;
        public ListsForComboboxes ListsForComboboxes { get; set; }
        public List<Account> ItemsForDebit { get; set; }

        public TranWithTags TranInWork { get; set; }

        public string MyAccountBalance { get { return _balanceDuringTransactionHinter.GetMyAccountBalance(TranInWork); } }

        [ImportingConstructor]
        public OneTransactionViewModel(KeeperDb db, AccountTreeStraightener accountTreeStraightener, BalanceDuringTransactionHinter balanceDuringTransactionHinter)
        {
            _db = db;
            _balanceDuringTransactionHinter = balanceDuringTransactionHinter;
            ListsForComboboxes = new ListsForComboboxes { FilterOnlyActiveAccounts = true };
            ListsForComboboxes.InitializeListsForCombobox(_db, accountTreeStraightener);
        }

        public void SetTran(TranWithTags tran)
        {
            TranInWork = tran;
            ItemsForDebit = ListsForComboboxes.ItemsForDebit;
        }

    }
}