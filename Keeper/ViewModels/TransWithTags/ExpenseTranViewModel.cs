using System.Composition;
using Caliburn.Micro;
using Keeper.Controls;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Transactions;
using System.Collections.Generic;
using System.Windows;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    class ExpenseTranViewModel : Screen, IOneTranView
    {
        public TestControlVm ModelForControl { get; set; }

        [ImportingConstructor]
        public ExpenseTranViewModel(KeeperDb db, BalanceDuringTransactionHinter balanceDuringTransactionHinter)
        {
            ModelForControl = new TestControlVm();
        }


        public void SetTran(TranWithTags tran)
        {
            ModelForControl.TextProperty = "Expense";
            ModelForControl.Buttons = new List<ButtonViewModel>
            {
                new ButtonViewModel("First", () => MessageBox.Show("blah")),
                new ButtonViewModel("Second", () => MessageBox.Show("bluh"))
            };
        }

        public void ButtonClose()
        {
            TryClose();
        }
    }
}