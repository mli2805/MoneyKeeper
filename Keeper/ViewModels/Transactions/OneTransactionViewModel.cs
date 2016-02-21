using System.Collections.Generic;
using System.Composition;
using Caliburn.Micro;
using Keeper.ByFunctional.AccountEditing;
using Keeper.DomainModel;

namespace Keeper.ViewModels.Transactions
{
    [Export]
    class OneTransactionViewModel : Screen
    {
        private readonly KeeperDb _db;
        public ListsForComboboxes ListsForComboboxes { get; set; }
        public Account SelectedItemDebit { get; set; }
        public List<Account> ItemsForDebit { get; set; }


        [ImportingConstructor]
        public OneTransactionViewModel(KeeperDb db, AccountTreeStraightener accountTreeStraightener)
        {
            _db = db;
            ListsForComboboxes = new ListsForComboboxes { FilterOnlyActiveAccounts = true };
            ListsForComboboxes.InitializeListsForCombobox(_db, accountTreeStraightener);
            ItemsForDebit = ListsForComboboxes.ItemsForDebit;
        }

    }
}