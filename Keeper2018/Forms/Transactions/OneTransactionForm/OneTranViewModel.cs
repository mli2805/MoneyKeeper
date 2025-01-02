using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class OneTranViewModel : Screen
    {
        public int Top { get; set; }
        private int _left;
        public int Left
        {
            get { return _left; }
            set
            {
                if (value == _left) return;
                _left = value;
                NotifyOfPropertyChange();
            }
        }
        public int Width { get; set; }
        public int Height { get; set; }

        private readonly IWindowManager _windowManager;
        private readonly KeeperDataModel _dataModel;
        private readonly ReceiptViewModel _receiptViewModel;
        private readonly FuellingInputViewModel _fuellingInputViewModel;

        private string _caption;
        private TransactionModel _tranInWork;
        public TransactionModel TranInWork
        {
            get { return _tranInWork; }
            set
            {
                if (Equals(value, _tranInWork)) return;
                _tranInWork = value;
                NotifyOfPropertyChange();
            }
        }
        public List<Tuple<decimal, AccountItemModel, string>> ReceiptList { get; set; }
        public TransactionModel FuellingTran { get; set; }
        public FuellingModel FuellingModel { get; set; }

        public bool IsAddMode { get; set; }
        public bool IsOneMore { get; set; }

        public UniversalControlVm MyIncomeControlVm { get; set; }
        public NewExpenseControlVm MyNewExpenseControlVm { get; set; }
        public UniversalControlVm MyTransferControlVm { get; set; }
        public UniversalControlVm MyExchangeControlVm { get; set; }

        public OperationTypeViewModel OperationTypeViewModel { get; } = new OperationTypeViewModel();

        public OneTranViewModel(IWindowManager windowManager, KeeperDataModel dataModel,
            ReceiptViewModel receiptViewModel, FuellingInputViewModel fuellingInputViewModel,
            UniversalControlVm myIncomeControlVm, NewExpenseControlVm myNewExpenseControlVm, 
            UniversalControlVm myTransferControlVm, UniversalControlVm myExchangeControlVm)
        {
            _windowManager = windowManager;
            _dataModel = dataModel;
            _receiptViewModel = receiptViewModel;
            _fuellingInputViewModel = fuellingInputViewModel;

            MyIncomeControlVm = myIncomeControlVm;
            MyNewExpenseControlVm = myNewExpenseControlVm;
            MyNewExpenseControlVm.PropertyChanged += MyNewExpenseControlVmOnPropertyChanged;
            MyTransferControlVm = myTransferControlVm;
            MyExchangeControlVm = myExchangeControlVm;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _caption;
        }

        public TransactionModel GetTran()
        {
            return TranInWork;
        }

        public void Init(TransactionModel tran, bool isAddMode)
        {
            ReceiptList = null;
            FuellingTran = null;
            IsAddMode = isAddMode;
            IsOneMore = false;
            _caption = isAddMode ? "Добавить" : "Изменить";
            OperationTypeViewModel.SelectedOperationType = tran.Operation;
            TranInWork = tran.Clone();
                
            InitControls();
            SetControlVisibilities(TranInWork.Operation);

            OperationTypeViewModel.PropertyChanged += OperationTypeViewModel_PropertyChanged;
        }

        private void OperationTypeViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TranInWork.Operation = OperationTypeViewModel.SelectedOperationType;
            ValidateTranInWorkFieldsWithNewOperationType();
            InitControls();
            SetControlVisibilities(OperationTypeViewModel.SelectedOperationType);
        }

        private void InitControls()
        {
            MyIncomeControlVm.SetTran(TranInWork);
            MyIncomeControlVm.IsAddMode = IsAddMode;
            MyNewExpenseControlVm.StartWith(TranInWork);
            MyNewExpenseControlVm.IsAddMode = IsAddMode;
            MyTransferControlVm.SetTran(TranInWork);
            MyTransferControlVm.IsAddMode = IsAddMode;
            MyExchangeControlVm.SetTran(TranInWork);
            MyExchangeControlVm.IsAddMode = IsAddMode;
        }
        
        private void SetControlVisibilities(OperationType opType)
        {
            MyIncomeControlVm.Visibility = opType == OperationType.Доход ? Visibility.Visible : Visibility.Collapsed;
            MyNewExpenseControlVm.Visibility = opType == OperationType.Расход ? Visibility.Visible : Visibility.Collapsed;
            MyTransferControlVm.Visibility = opType == OperationType.Перенос ? Visibility.Visible : Visibility.Collapsed;
            MyExchangeControlVm.Visibility = opType == OperationType.Обмен ? Visibility.Visible : Visibility.Collapsed;
        }

        private void MyNewExpenseControlVmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "ForParentView") return;
            if (MyNewExpenseControlVm.ForParentView == "Fuelling") Fuelling();
            if (MyNewExpenseControlVm.ForParentView == "Receipt") Receipt();
        }


        private void ValidateTranInWorkFieldsWithNewOperationType()
        {
            TranInWork.Tags.Clear();
            if (TranInWork.MySecondAccount == null && (TranInWork.Operation == OperationType.Перенос || TranInWork.Operation == OperationType.Обмен))
                TranInWork.MySecondAccount = _dataModel.AcMoDict[163];
            if (TranInWork.CurrencyInReturn == null && TranInWork.Operation == OperationType.Обмен)
                TranInWork.CurrencyInReturn = (TranInWork.Currency == CurrencyCode.BYN) ? CurrencyCode.USD : CurrencyCode.BYN;
        }

        private void CleanUnneccessaryProperties()
        {
            if (TranInWork.Operation == OperationType.Доход || TranInWork.Operation == OperationType.Расход)
            {
                TranInWork.MySecondAccount = null;
            }
            if (TranInWork.Operation != OperationType.Обмен)
            {
                TranInWork.AmountInReturn = 0;
                TranInWork.CurrencyInReturn = null;
            }

            if (TranInWork.Operation == OperationType.Перенос || TranInWork.Operation == OperationType.Обмен)
            {
                TranInWork.Counterparty = null;
                TranInWork.Category = null;
            }

            if (TranInWork.Operation != OperationType.Расход)
            {
                TranInWork.PaymentWay = PaymentWay.НеЗадано;
            }
        }

        private bool IsValid()
        {
            //if (ReceiptList == null && TranInWork.HasntGotCategoryTagThoughItShould()) return false;
          
            if (TranInWork.Operation == OperationType.Расход && TranInWork.PaymentWay == PaymentWay.НеЗадано)
            {
                MessageBox.Show("Не задан способ оплаты!", "Ошибка!");
                return false;
            }
            /* more checks
             * ...
            */
            return true;
        }

        public void Calculator()
        {
            System.Diagnostics.Process.Start("calc");
        }

        public void Save()
        {
            if (!IsValid()) return;
            CleanUnneccessaryProperties();
            TryClose(true);
        }

        public void OneMore()
        {
            IsOneMore = true;
            Save();
        }

        public void Cancel()
        {
            TryClose(false);
        }

        public void Receipt()
        {
            if (Left > 600) Left -= 180;
            _receiptViewModel.Initialize(TranInWork.Amount, TranInWork.Currency, _dataModel.AcMoDict[256]);
            _receiptViewModel.PlaceIt(Top, Left + Width, Height);

            if (_windowManager.ShowDialog(_receiptViewModel) != true) return;

            ReceiptList = _receiptViewModel.ResultList;
            Save();
        }

        public void Fuelling()
        {
            if (Left > 600) Left -= 180;
            if (_dataModel.Cars == null)
            {
                MessageBox.Show("Cars должны быть заполнены!");
                return;
            }
            _fuellingInputViewModel.Initialize(CreateNewFuelling());
            _fuellingInputViewModel.PlaceIt(Top, Left + Width, Height);

            if (_windowManager.ShowDialog(_fuellingInputViewModel) != true) return;
            FuellingTran = CreateFuellingTran(_fuellingInputViewModel.Vm);
            FuellingModel = CreateFuellingModel(FuellingTran, _fuellingInputViewModel.Vm);

            TryClose(true);
        }

        private FuellingModel CreateNewFuelling()
        {
            return new FuellingModel()
            {
                CarAccountId = _dataModel.Cars.Last().CarAccountId,
                Timestamp = TranInWork.Timestamp,
                Volume = 30,
                FuelType = FuelType.ДтЕвро5,
                Amount = TranInWork.Amount,
                Currency = TranInWork.Currency,
                Comment = TranInWork.Comment,
            };
        }

        private TransactionModel CreateFuellingTran(FuellingInputVm vm)
        {
            var carAccount = _dataModel.AcMoDict[vm.CarAccountId];
            var thisCarFuel = (AccountItemModel)carAccount.Children.First(c => c.Name.Contains("авто топливо"));
            var azs = _dataModel.AcMoDict[272];
            return new TransactionModel()
            {
                Operation = OperationType.Расход,
                PaymentWay = PaymentWay.ПриложениеПродавца,
                Timestamp = vm.Timestamp,
                Counterparty = azs,
                Category = thisCarFuel,
                Amount = vm.Amount,
                Currency = vm.Currency,
                Tags = new List<AccountItemModel>(){ _dataModel.AcMoDict[1066]},
                Comment = $"{vm.Volume} л {vm.FuelType} ({vm.Comment})",
            };
        }

        private FuellingModel CreateFuellingModel(TransactionModel transactionModel, FuellingInputVm vm)
        {
            var maxId = _dataModel.FuellingVms.Max(f => f.Id);
            return new FuellingModel()
            {
                Id = maxId + 1,
                Transaction = transactionModel,

                Timestamp = vm.Timestamp,
                Volume = vm.Volume,
                CarAccountId = vm.CarAccountId,
                FuelType = vm.FuelType,
                Amount = vm.Amount,
                Currency = vm.Currency,
                Comment = vm.Comment,
                OneLitrePrice = vm.OneLitrePrice,
                OneLitreInUsd = vm.OneLitreInUsd,
            };
        }
    }
}
