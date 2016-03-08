using System.Composition;
using Caliburn.Micro;
using Keeper.Controls;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Transactions;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Keeper.Controls.AccNameSelectionControl;
using Keeper.DomainModel.WorkTypes;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    class ExpenseTranViewModel : Screen, IOneTranView
    {
        public AccNameSelectorVm ModelForControl { get; set; }
        public TranWithTags TranInWork { get; set; }
        public AccName MyAccName { get; set; }

        [ImportingConstructor]
        public ExpenseTranViewModel(KeeperDb db, BalanceDuringTransactionHinter balanceDuringTransactionHinter)
        {
            ModelForControl = new AccNameSelectorVm();
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
            MyAccName = ListsForComboTrees.MyAccNamesForExpense.FindThroughTheForest(TranInWork.MyAccount.Name)
                          ?? ListsForComboTrees.MyAccNamesForExpense.FindThroughTheForest("Мой кошелек");

            InitializeOutWalletControl();
        }

        private void InitializeOutWalletControl()
        {
            ModelForControl.ControlTitle = "Откуда";
            ModelForControl.Buttons = new List<AccNameButtonVm>
            {
                new AccNameButtonVm("мк", () => MessageBox.Show("blah")),
                new AccNameButtonVm("биб", () => MessageBox.Show("blah")),
                new AccNameButtonVm("газ", () => MessageBox.Show("blah")),
                new AccNameButtonVm("юк", () => MessageBox.Show("bluh"))
            };
            ModelForControl.AccNamesListForExpense = ListsForComboTrees.MyAccNamesForExpense;
            ModelForControl.MyAccName = MyAccName;
        }

        private void TranInWork_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }

        public void ButtonClose()
        {
            TryClose();
        }
    }
}