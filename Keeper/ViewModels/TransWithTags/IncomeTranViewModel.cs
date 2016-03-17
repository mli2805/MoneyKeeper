using System.Composition;
using Caliburn.Micro;
using Keeper.Controls;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Transactions;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    class IncomeTranViewModel : Screen, IOneTranView
    {

        public TranWithTags TranInWork { get; set; }
        public IncomeControlVm MyIncomeControlVm { get; set; }

        [ImportingConstructor]
        public IncomeTranViewModel()
        {
            MyIncomeControlVm = IoC.Get<IncomeControlVm>();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Income transaction with tags";
        }

        public TranWithTags GetTran()
        {
            return TranInWork;
        }
        public void SetTran(TranWithTags tran)
        {
            TranInWork = tran.Clone();
            MyIncomeControlVm.SetTran(TranInWork);
        }
        public void Save()
        {
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }
    }
}
