using System;
using System.Collections.Generic;
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
        public List<Tuple<decimal, AccountModel, string>> ReceiptList { get; set; }
        public TransactionModel FuellingTran { get; set; }

        public bool IsAddMode { get; set; }
        public bool IsOneMore { get; set; }

        public UniversalControlVm MyIncomeControlVm { get; set; }
        public UniversalControlVm MyExpenseControlVm { get; set; }
        public UniversalControlVm MyTransferControlVm { get; set; }
        public UniversalControlVm MyExchangeControlVm { get; set; }

        public OperationTypeViewModel OperationTypeViewModel { get; } = new OperationTypeViewModel();

        public OneTranViewModel(IWindowManager windowManager, KeeperDataModel dataModel,
            ReceiptViewModel receiptViewModel, FuellingInputViewModel fuellingInputViewModel,
            UniversalControlVm myIncomeControlVm, UniversalControlVm myExpenseControlVm,
            UniversalControlVm myTransferControlVm, UniversalControlVm myExchangeControlVm)
        {
            _windowManager = windowManager;
            _dataModel = dataModel;
            _receiptViewModel = receiptViewModel;
            _fuellingInputViewModel = fuellingInputViewModel;

            MyIncomeControlVm = myIncomeControlVm;
            MyExpenseControlVm = myExpenseControlVm;
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

        private void OperationTypeViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            TranInWork.Operation = OperationTypeViewModel.SelectedOperationType;
            ValidateTranInWorkFieldsWithNewOperationType();
            InitControls();
            SetControlVisibilities(OperationTypeViewModel.SelectedOperationType);
        }

        private void ValidateTranInWorkFieldsWithNewOperationType()
        {
            TranInWork.Tags.Clear();
            if (TranInWork.MySecondAccount == null && (TranInWork.Operation == OperationType.Перенос || TranInWork.Operation == OperationType.Обмен))
                TranInWork.MySecondAccount = _dataModel.AcMoDict[163];
            if (TranInWork.CurrencyInReturn == null && TranInWork.Operation == OperationType.Обмен)
                TranInWork.CurrencyInReturn = (TranInWork.Currency == CurrencyCode.BYN) ? CurrencyCode.USD : CurrencyCode.BYN;
        }

        private bool IsValid()
        {
            if (ReceiptList == null && TranInWork.HasntGotCategoryTagThoughItShould()) return false;
            if (TranInWork.Operation == OperationType.Доход || TranInWork.Operation == OperationType.Расход)
            {
                TranInWork.MySecondAccount = null;
            }
            if (TranInWork.Operation != OperationType.Обмен)
            {
                TranInWork.AmountInReturn = 0;
                TranInWork.CurrencyInReturn = null;
            }

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

        public void Save()
        {
            if (!IsValid()) return;
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

        private void InitControls()
        {
            MyIncomeControlVm.SetTran(TranInWork);
            MyExpenseControlVm.SetTran(TranInWork);
            MyTransferControlVm.SetTran(TranInWork);
            MyExchangeControlVm.SetTran(TranInWork);
        }
        private void SetControlVisibilities(OperationType opType)
        {
            MyIncomeControlVm.Visibility = opType == OperationType.Доход ? Visibility.Visible : Visibility.Collapsed;
            MyExpenseControlVm.Visibility = opType == OperationType.Расход ? Visibility.Visible : Visibility.Collapsed;
            MyTransferControlVm.Visibility = opType == OperationType.Перенос ? Visibility.Visible : Visibility.Collapsed;
            MyExchangeControlVm.Visibility = opType == OperationType.Обмен ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool LeaveOneExternalAccountInTags()
        {
            var externalAccount = TranInWork.GetExternalAccount();
            if (externalAccount == null) return false;
            TranInWork.Tags = new List<AccountModel>() { externalAccount };
            InitControls();
            return true;
        }

        public void Receipt()
        {
            if (!LeaveOneExternalAccountInTags()) return;

            Left = Left - 180;
            _receiptViewModel.Initialize(TranInWork.Amount, TranInWork.Currency, _dataModel.AcMoDict[256]);
            _receiptViewModel.PlaceIt(Top, Left + Width, Height);

            if (_windowManager.ShowDialog(_receiptViewModel) != true) return;

            ReceiptList = _receiptViewModel.ResultList;
            Save();
        }

        public void Fuelling()
        {
            Left = Left - 180;
            if (_dataModel.Cars == null)
            {
                MessageBox.Show("Cars должны быть заполнены!");
                return;
            }
            _fuellingInputViewModel.Initialize(CreateNewFuelling());
            _fuellingInputViewModel.PlaceIt(Top, Left + Width, Height);

            if (_windowManager.ShowDialog(_fuellingInputViewModel) != true) return;
            CreateFuellingTran(_fuellingInputViewModel.Vm);

            TryClose(true);
        }

        private Fuelling CreateNewFuelling()
        {
            return new Fuelling()
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

        private void CreateFuellingTran(FuellingInputVm vm)
        {
            var carAccount = _dataModel.AcMoDict[vm.CarAccountId];
            var account = carAccount.Children.First(c => c.Name.Contains("авто топливо"));
            var azs = _dataModel.AcMoDict[272];
            FuellingTran = new TransactionModel()
            {
                Operation = OperationType.Расход,
                Timestamp = vm.Timestamp,
                Amount = vm.Amount,
                Currency = vm.Currency,
                Tags = new List<AccountModel>() { account, azs, },
                Comment = $"{vm.Volume} л {vm.FuelType} ({vm.Comment})",
            };
        }
    }
}
