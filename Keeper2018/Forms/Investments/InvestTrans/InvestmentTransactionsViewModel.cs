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

        public void ActionsMethod(TranAction action)
        {
            switch (action)
            {
                case TranAction.Edit:
                    EditSelected();
                    return;
                case TranAction.AddAfterSelected:
                    AddNewTran();
                    return;
                case TranAction.Delete:
                    DeleteSelected();
                    return;
            }
        }

        public void EditSelected()
        {
            var vm = _globalScope.Resolve<OneInvestTranViewModel>();
            var tranInWork = SelectedTransaction.ShallowCopy();
            vm.Initialize(tranInWork);
            bool? result = _windowManager.ShowDialog(vm);
            if (result == true)
            {
                SelectedTransaction.CopyFieldsFrom(tranInWork);
            }
        }

        public void AddNewTran()
        {

        }

        public void TopUpTrustAccount()
        {
            var vm = _globalScope.Resolve<OneInvestTranViewModel>();
            var tranInWork = new InvestTranModel()
            {
                Id = Transactions.Any() ? Transactions.Max(t => t.Id) + 1 : 1,
                InvestOperationType = InvestOperationType.TopUpTrustAccount,
                Timestamp = DateTime.Today,
                Currency = CurrencyCode.USD,
            };
            vm.Initialize(tranInWork);
            bool? result = _windowManager.ShowDialog(vm);
            if (result == true)
            {
                Transactions.Add(tranInWork);
            }
        }

        public void BuyStocks()
        {
            var vm = _globalScope.Resolve<OneInvestTranViewModel>();
            var tranInWork = new InvestTranModel()
            {
                Id = Transactions.Any() ? Transactions.Max(t => t.Id) + 1 : 1,
                InvestOperationType = InvestOperationType.BuyStocks,
                Timestamp = DateTime.Today,
                Currency = CurrencyCode.USD,
            };
            vm.Initialize(tranInWork);
            bool? result = _windowManager.ShowDialog(vm);
            if (result == true)
            {
                Transactions.Add(tranInWork);
            }
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
