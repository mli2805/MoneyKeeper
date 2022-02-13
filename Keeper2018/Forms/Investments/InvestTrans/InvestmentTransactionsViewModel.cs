using System;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{

    public class InvestmentTransactionsViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly KeeperDataModel _dataModel;
        private readonly IWindowManager _windowManager;

        public ObservableCollection<InvestTranModel> Transactions { get; set; }
        public InvestTranModel SelectedTransaction { get; set; }

        public InvestmentTransactionsViewModel(ILifetimeScope globalScope, KeeperDataModel dataModel, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _dataModel = dataModel;
            _windowManager = windowManager;
        }

        public void Initialize()
        {
            Transactions = new ObservableCollection<InvestTranModel>(_dataModel.InvestTranModels);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Проводки";
        }

        public void DeleteSelected()
        {
            if (SelectedTransaction != null)
                Transactions.Remove(SelectedTransaction);
        }

        public void TopUpTrustAccount()
        {
            var vm = _globalScope.Resolve<OneInvestTranViewModel>();
            vm.Initialize(new InvestTranModel() { InvestOperationType = InvestOperationType.TopUpTrustAccount });
            _windowManager.ShowDialog(vm);
        }

        public void Close()
        {
            TryClose();
        }

        public override void CanClose(Action<bool> callback)
        {
            _dataModel.InvestTranModels = Transactions.ToList();
            base.CanClose(callback);
        }
    }
}
