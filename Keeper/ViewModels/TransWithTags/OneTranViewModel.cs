using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Keeper.Controls.OneTranViewControls;
using Keeper.Controls.OneTranViewControls.SubControls;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Extentions;
using Keeper.DomainModel.Trans;
using Keeper.ViewModels.SingleViews;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    class OneTranViewModel : Screen
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

        public static IWindowManager WindowManager => IoC.Get<IWindowManager>();

        private readonly KeeperDb _db;
        
        private string _caption;
        private TranWithTags _tranInWork;
        public TranWithTags TranInWork
        {
            get { return _tranInWork; }
            set
            {
                if (Equals(value, _tranInWork)) return;
                _tranInWork = value;
                NotifyOfPropertyChange();
            }
        }
        public List<Tuple<decimal, Account, string>> ReceiptList { get; set; }

        public bool IsAddOrEdit { get; set; }
        public bool IsOneMore { get; set; } = false;

        public UniversalControlVm MyIncomeControlVm { get; set; } = IoC.Get<UniversalControlVm>();
        public UniversalControlVm MyExpenseControlVm { get; set; } = IoC.Get<UniversalControlVm>();
        public UniversalControlVm MyTransferControlVm { get; set; } = IoC.Get<UniversalControlVm>();
        public UniversalControlVm MyExchangeControlVm { get; set; } = IoC.Get<UniversalControlVm>();
        public OpTypeChoiceControlVm MyOpTypeChoiceControlVm { get; set; } = new OpTypeChoiceControlVm();

        [ImportingConstructor]
        public OneTranViewModel(KeeperDb db)
        {
            _db = db;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _caption;
        }

        public TranWithTags GetTran()
        {
            return TranInWork;
        }

        public void Init(TranWithTags tran, bool AddOrEdit)
        {
            IsAddOrEdit = AddOrEdit;
            _caption = AddOrEdit ? "Добавить" : "Изменить";
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
                TranInWork.CurrencyInReturn = (TranInWork.Currency == CurrencyCodes.BYN) ? CurrencyCodes.USD : CurrencyCodes.BYN;
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
            TranInWork.Tags = new List<Account>() {externalAccount};
            InitCorrespondingControl();
            return true;
        }

        public void Receipt()
        {
            if ( ! LeaveOneExternalAccountInTags()) return;

            Left = Left - 180;
            var receiptVm = IoC.Get<ReceiptViewModel>();
            receiptVm.Initialize(TranInWork.Amount, TranInWork.Currency.GetValueOrDefault(), _db.SeekAccount("Прочие расходы"));
            receiptVm.PlaceIt(Top, Left + Width, Height);

            if (WindowManager.ShowDialog(receiptVm) != true) return;

            ReceiptList = receiptVm.ResultList;
            Save();
        }
    }
}
