using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;

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
        private readonly KeeperDb _db;
        
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

        public bool IsAddOrEdit { get; set; }
        public bool IsOneMore { get; set; }

        public UniversalControlVm MyIncomeControlVm { get; set; } = IoC.Get<UniversalControlVm>();
        public UniversalControlVm MyExpenseControlVm { get; set; } = IoC.Get<UniversalControlVm>();
        public UniversalControlVm MyTransferControlVm { get; set; } = IoC.Get<UniversalControlVm>();
        public UniversalControlVm MyExchangeControlVm { get; set; } = IoC.Get<UniversalControlVm>();
        public OpTypeChoiceControlVm MyOpTypeChoiceControlVm { get; set; } = new OpTypeChoiceControlVm();

        public OneTranViewModel(IWindowManager windowManager, KeeperDb db)
        {
            _windowManager = windowManager;
            _db = db;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _caption;
        }

        public TransactionModel GetTran()
        {
            return TranInWork;
        }

        public void Init(TransactionModel tran, bool addOrEdit)
        {
            IsAddOrEdit = addOrEdit;
            _caption = addOrEdit ? "Добавить" : "Изменить";
            TranInWork = tran.Clone();

            SetVisibility(TranInWork.Operation);
            InitCorrespondingControl();

            MyOpTypeChoiceControlVm.PressedButton = TranInWork.Operation;
            MyOpTypeChoiceControlVm.PropertyChanged += MyOpTypeChoiceControlVm_PropertyChanged;
        }
        private void MyOpTypeChoiceControlVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            TranInWork.Operation = ((OpTypeChoiceControlVm)sender).PressedButton;
            ValidateTranInWorkFieldsWithNewOperationType();
            SetVisibility(TranInWork.Operation);
            InitCorrespondingControl();
        }

        private void ValidateTranInWorkFieldsWithNewOperationType()
        {
            TranInWork.Tags.Clear();
            if (TranInWork.MySecondAccount == null && TranInWork.Operation == OperationType.Перенос)
                TranInWork.MySecondAccount = _db.SeekAccount("Юлин кошелек");
            if (TranInWork.CurrencyInReturn == null)
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

        private void InitCorrespondingControl()
        {
            MyIncomeControlVm.SetTran(TranInWork);
            MyExpenseControlVm.SetTran(TranInWork);
            MyTransferControlVm.SetTran(TranInWork);
            MyExchangeControlVm.SetTran(TranInWork);
        }
        private void SetVisibility(OperationType opType)
        {
            MyIncomeControlVm.Visibility = opType == OperationType.Доход ? Visibility.Visible : Visibility.Collapsed;
            MyExpenseControlVm.Visibility = opType == OperationType.Расход ? Visibility.Visible : Visibility.Collapsed;
            MyTransferControlVm.Visibility = opType == OperationType.Перенос ? Visibility.Visible : Visibility.Collapsed;
            MyExchangeControlVm.Visibility = opType == OperationType.Обмен ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool LeaveOneExternalAccountInTags()
        {
            var externalAccount = TranInWork.Tags.FirstOrDefault(a => a.Is("Внешние"));
            if (externalAccount == null)
            {
                MessageBox.Show("Должен быть хотя бы один продавец/услугодатель");
                return false;
            }
            TranInWork.Tags = new List<AccountModel>() {externalAccount};
            InitCorrespondingControl();
            return true;
        }

        public void Receipt()
        {
            if ( ! LeaveOneExternalAccountInTags()) return;

            Left = Left - 180;
            var receiptVm = new ReceiptViewModel();
            receiptVm.Initialize(TranInWork.Amount, TranInWork.Currency, _db.SeekAccount("Прочие расходы"));
            receiptVm.PlaceIt(Top, Left + Width, Height);

            if (_windowManager.ShowDialog(receiptVm) != true) return;

            ReceiptList = receiptVm.ResultList;
            Save();
        }
    }
}
