using System.Composition;
using Caliburn.Micro;
using Keeper.Controls;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Transactions;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Keeper.DomainModel.WorkTypes;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    class ExpenseTranViewModel : Screen, IOneTranView
    {
        public TestControlVm ModelForControl { get; set; }
        public TranWithTags TranInWork { get; set; }
        public AccName MyAccName { get; set; }

        [ImportingConstructor]
        public ExpenseTranViewModel(KeeperDb db, BalanceDuringTransactionHinter balanceDuringTransactionHinter)
        {
            ModelForControl = new TestControlVm();
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
            ModelForControl.Buttons = new List<ButtonViewModel>
            {
                new ButtonViewModel("мк", () => MessageBox.Show("blah")),
                new ButtonViewModel("биб", () => MessageBox.Show("blah")),
                new ButtonViewModel("газ", () => MessageBox.Show("blah")),
                new ButtonViewModel("юк", () => MessageBox.Show("bluh"))
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