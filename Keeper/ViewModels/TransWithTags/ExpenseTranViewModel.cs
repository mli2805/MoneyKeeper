using System;
using System.Composition;
using Caliburn.Micro;
using Keeper.Controls;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Transactions;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Keeper.Controls.AccNameSelectionControl;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.AccountEditing;
using Microsoft.Vbe.Interop;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    class ExpenseTranViewModel : Screen, IOneTranView
    {
        private readonly KeeperDb _db;
        private readonly AccountTreeStraightener _accountTreeStraightener;
        private readonly BalanceDuringTransactionHinter _balanceDuringTransactionHinter;
        public TranWithTags TranInWork { get; set; }
        public AccNameSelectorVm MyAccNameSelectorVm { get; set; }
        public List<CurrencyCodes> Currencies { get; set; } = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();

        public string MyAccountBalance { get {return _balanceDuringTransactionHinter.GetMyAccountBalance(TranInWork); } }

        [ImportingConstructor]
        public ExpenseTranViewModel(KeeperDb db, AccountTreeStraightener accountTreeStraightener, BalanceDuringTransactionHinter balanceDuringTransactionHinter)
        {
            _db = db;
            _accountTreeStraightener = accountTreeStraightener;
            _balanceDuringTransactionHinter = balanceDuringTransactionHinter;
            MyAccNameSelectorVm = new AccNameSelectorVm();
            ListsForComboTrees.InitializeLists(db);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Расход";
        }

        public void SetTran(TranWithTags tran)
        {
            TranInWork = tran.Clone();
            TranInWork.PropertyChanged += TranInWork_PropertyChanged;

            InitializeMyAccNameSelectionControl();
            MyAccNameSelectorVm.PropertyChanged += MyAccNameSelectorVm_PropertyChanged;
        }

        private void MyAccNameSelectorVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MyAccName")
                TranInWork.MyAccount = _accountTreeStraightener.Seek(MyAccNameSelectorVm.MyAccName.Name, _db.Accounts);
        }

        private void InitializeMyAccNameSelectionControl()
        {
            MyAccNameSelectorVm.ControlTitle = "Откуда";
            MyAccNameSelectorVm.Buttons = new List<AccNameButtonVm>
            {
                new AccNameButtonVm("мк",  ListsForComboTrees.MyAccNamesForExpense.FindThroughTheForest("Мой кошелек")),
                new AccNameButtonVm("биб", ListsForComboTrees.MyAccNamesForExpense.FindThroughTheForest("БИБ Сберка Моцная")),
                new AccNameButtonVm("газ", ListsForComboTrees.MyAccNamesForExpense.FindThroughTheForest("БГПБ Сберегательная")),
                new AccNameButtonVm("юк",  ListsForComboTrees.MyAccNamesForExpense.FindThroughTheForest("Юлин кошелек"))
            };
            MyAccNameSelectorVm.AccNamesListForExpense = ListsForComboTrees.MyAccNamesForExpense;
            MyAccNameSelectorVm.MyAccName = ListsForComboTrees.MyAccNamesForExpense.FindThroughTheForest(TranInWork.MyAccount.Name)
                                            ?? ListsForComboTrees.MyAccNamesForExpense.FindThroughTheForest("Мой кошелек");
        }

        private void TranInWork_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            NotifyOfPropertyChange(nameof(MyAccountBalance));
        }

        public void ButtonClose()
        {
            TryClose();
        }
    }
}