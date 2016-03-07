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
        public TestControlVm TestControlVm { get; set; }

        [ImportingConstructor]
        public ExpenseTranViewModel(KeeperDb db, BalanceDuringTransactionHinter balanceDuringTransactionHinter)
        {
            TestControlVm = new TestControlVm();
        }


        public void SetTran(TranWithTags tran)
        {
            TestControlVm.TextProperty = "Expense";
        }

        public void Close()
        {
            TryClose();
        }
    }
}