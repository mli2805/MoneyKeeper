using System.Composition;
using Caliburn.Micro;
using Keeper.Controls;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Transactions;

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
        }

        public void ButtonClose()
        {
            TryClose();
        }
    }
}