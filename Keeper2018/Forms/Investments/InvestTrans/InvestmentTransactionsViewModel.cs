using System;
using System.Collections.Generic;
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

        public InvestmentTransactionsViewModel(ILifetimeScope globalScope, KeeperDataModel dataModel,
            IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _dataModel = dataModel;
            _windowManager = windowManager;
        }

        public void Initialize()
        {
            Transactions = new ObservableCollection<InvestTranModel>(_dataModel.InvestTranModels);
            SelectedTransaction = Transactions.Last();
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

        // Buttons to add specific invest operation
        public void InvestOperation(InvestOperationType investOperationType)
        {
            var vm = _globalScope.Resolve<OneInvestTranViewModel>();
            var tranInWork = new InvestTranModel()
            {
                Id = Transactions.Any() ? Transactions.Max(t => t.Id) + 1 : 1,
                InvestOperationType = investOperationType,
                Timestamp = DateTime.Today,
                Currency = CurrencyCode.USD,
            };
            vm.Initialize(tranInWork);
            bool? result = _windowManager.ShowDialog(vm);
            if (result == true)
            {
                Transactions.Add(tranInWork);

                if (ToOperationType(tranInWork.InvestOperationType, out OperationType operationType))
                {
                    var tran = InvestTranModelToTransactionModel(tranInWork, operationType);
                    _dataModel.Transactions.Add(tran.Id, tran);
                }
            }
        }

        private TransactionModel InvestTranModelToTransactionModel(InvestTranModel tranInWork, OperationType operationType)
        {
            var tran = new TransactionModel();
            tran.Id = _dataModel.Transactions.Keys.Max() + 1;

            var lastInDate =
                _dataModel.Transactions.Values.LastOrDefault(t =>
                    t.Timestamp.Date == tranInWork.Timestamp.Date);
            tran.Timestamp = lastInDate == null ? tranInWork.Timestamp.AddMinutes(1) : lastInDate.Timestamp.AddMinutes(1);

            tran.Operation = operationType;

            if (operationType == OperationType.Расход) // комиссии
            {
                tran.PaymentWay = PaymentWay.КартаДругое;
                tran.MyAccount = _dataModel.AcMoDict[tranInWork.TrustAccount.AccountId];
                tran.Tags = new List<AccountModel>()
                {
                    _dataModel.AcMoDict[694], // Альфа
                    _dataModel.AcMoDict[tranInWork.InvestOperationType == InvestOperationType.PayBaseCommission ? 896 : 897]
                };
            }
            else if (operationType == OperationType.Доход) // дивиденды
            {
                tran.MyAccount = _dataModel.AcMoDict[tranInWork.TrustAccount.AccountId];
                tran.Tags = new List<AccountModel>()
                {
                    tranInWork.AccountModel, // Альфа
                    _dataModel.AcMoDict[209] // дивиденды
                };
            }
            else if (operationType == OperationType.Перенос)  // пополнение-вывод
            {
                if (tranInWork.InvestOperationType == InvestOperationType.TopUpTrustAccount)
                {
                    tran.MyAccount = tranInWork.AccountModel;
                    tran.MySecondAccount = _dataModel.AcMoDict[tranInWork.TrustAccount.AccountId];
                }
                else //  InvestOperationType.WithdrawFromTrustAccount
                {
                    tran.MyAccount = _dataModel.AcMoDict[tranInWork.TrustAccount.AccountId];
                    tran.MySecondAccount = tranInWork.AccountModel;
                }
                tran.Tags = new List<AccountModel>()
                {
                    _dataModel.AcMoDict[694], // Альфа
                };
            }

            tran.Amount = tranInWork.CurrencyAmount;
            tran.Currency = tranInWork.Currency;

            tran.Comment = tranInWork.InvestOperationType == InvestOperationType.PayBaseCommission
                ? (tranInWork.TrustAccount.Id == 1
                      ? "по долларовому счету"
                      : "по рублевому счету")
                  + tranInWork.Comment
                : tranInWork.Comment;

            return tran;
        }

        private bool ToOperationType(InvestOperationType investOperationType, out OperationType operationType)
        {
            switch (investOperationType)
            {
                case InvestOperationType.PayBaseCommission:
                case InvestOperationType.PayBuySellFee:
                    operationType = OperationType.Расход;
                    return true;

                case InvestOperationType.EnrollCouponOrDividends:
                    operationType = OperationType.Доход;
                    return true;

                case InvestOperationType.TopUpTrustAccount:
                case InvestOperationType.WithdrawFromTrustAccount:
                    operationType = OperationType.Перенос;
                    return true;

                default:
                    operationType = OperationType.Перенос;
                    return false;
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
