using System.Composition;
using Caliburn.Micro;
using Keeper.Controls;
using Keeper.DomainModel.Transactions;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    class IncomeTranViewModel : Screen, IOneTranView
    {
        public TranWithTags TranInWork { get; set; }
        public IncomeControlVm MyIncomeControlVm { get; set; }
        public OpTypeChoiceControlVm MyOpTypeChoiceControlVm { get; set; } = new OpTypeChoiceControlVm();

        [ImportingConstructor]
        public IncomeTranViewModel()
        {
            MyIncomeControlVm = IoC.Get<IncomeControlVm>();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "";
        }

        public TranWithTags GetTran()
        {
            return TranInWork;
        }
        public void SetTran(TranWithTags tran)
        {
            TranInWork = tran.Clone();
            MyIncomeControlVm.SetTran(TranInWork);
            MyOpTypeChoiceControlVm.PressedButton = TranInWork.Operation;
            MyOpTypeChoiceControlVm.PropertyChanged += MyOpTypeChoiceControlVm_PropertyChanged;
        }

        private void MyOpTypeChoiceControlVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DisplayName = "Расход";
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
