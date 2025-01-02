using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class UniversalControlVm : PropertyChangedBase
    {
        private Visibility _visibility;
        public Visibility Visibility
        {
            get => _visibility;
            set
            {
                if (value == _visibility) return;
                _visibility = value;
                NotifyOfPropertyChange();
            }
        }

        private readonly KeeperDataModel _dataModel;
        private readonly AccNameSelector _accNameSelectionControlInitializer;
        private readonly BalanceDuringTransactionHinter _balanceDuringTransactionHinter;

        #region TranInWork Properties
        public TransactionModel TranInWork { get; set; } = new TransactionModel();
        public bool IsAddMode { get; set; }

        private AccNameSelectorVm _myAccNameSelectorVm;
        public AccNameSelectorVm MyAccNameSelectorVm
        {
            get => _myAccNameSelectorVm;
            set
            {
                if (Equals(value, _myAccNameSelectorVm)) return;
                _myAccNameSelectorVm = value;
                NotifyOfPropertyChange();
            }
        }

        private AccNameSelectorVm _counterpartySelectorVm;
        public AccNameSelectorVm CounterpartySelectorVm
        {
            get => _counterpartySelectorVm;
            set
            {
                if (Equals(value, _counterpartySelectorVm)) return;
                _counterpartySelectorVm = value;
                NotifyOfPropertyChange();
            }
        }

        private AccNameSelectorVm _categorySelectorVm;
        public AccNameSelectorVm CategorySelectorVm
        {
            get => _categorySelectorVm;
            set
            {
                if (Equals(value, _categorySelectorVm)) return;
                _categorySelectorVm = value;
                NotifyOfPropertyChange();
            }
        }


        private AccNameSelectorVm _mySecondAccNameSelectorVm;
        public AccNameSelectorVm MySecondAccNameSelectorVm
        {
            get => _mySecondAccNameSelectorVm;
            set
            {
                if (Equals(value, _mySecondAccNameSelectorVm)) return;
                _mySecondAccNameSelectorVm = value;
                NotifyOfPropertyChange();
            }
        }
        private AmountInputControlVm _myAmountInputControlVm;
        public AmountInputControlVm MyAmountInputControlVm
        {
            get => _myAmountInputControlVm;
            set
            {
                if (Equals(value, _myAmountInputControlVm)) return;
                _myAmountInputControlVm = value;
                NotifyOfPropertyChange();
            }
        }
        private AmountInputControlVm _myAmountInReturnInputControlVm;
        public AmountInputControlVm MyAmountInReturnInputControlVm
        {
            get => _myAmountInReturnInputControlVm;
            set
            {
                if (Equals(value, _myAmountInReturnInputControlVm)) return;
                _myAmountInReturnInputControlVm = value;
                NotifyOfPropertyChange();
            }
        }

        private TagPickerVm _myTagPickerVm;
        public TagPickerVm MyTagPickerVm
        {
            get => _myTagPickerVm;
            set
            {
                if (Equals(value, _myTagPickerVm)) return;
                _myTagPickerVm = value;
                NotifyOfPropertyChange();
            }
        }
        private DatePickerWithTrianglesVm _myDatePickerVm;
        public DatePickerWithTrianglesVm MyDatePickerVm
        {
            get => _myDatePickerVm;
            set
            {
                if (Equals(value, _myDatePickerVm)) return;
                _myDatePickerVm = value;
                NotifyOfPropertyChange();
            }
        }

        private string _myAccountBalance;
        public string MyAccountBalance
        {
            get => _myAccountBalance;
            set
            {
                if (value == _myAccountBalance) return;
                _myAccountBalance = value;
                NotifyOfPropertyChange();
            }
        }

        public string MySecondAccountBalance => _balanceDuringTransactionHinter.GetMySecondAccountBalance(TranInWork);
        public string AmountInUsd => _balanceDuringTransactionHinter.GetAmountInUsd(TranInWork);
        public string AmountInReturnInUsd => _balanceDuringTransactionHinter.GetAmountInReturnInUsd(TranInWork);
        public string ExchangeRate => _balanceDuringTransactionHinter.GetExchangeRate(TranInWork);

        public List<PaymentWay> PaymentWays { get; set; }

        private PaymentWay _selectedPaymentWay;

        public PaymentWay SelectedPaymentWay
        {
            get => _selectedPaymentWay;
            set
            {
                _selectedPaymentWay = value;
                TranInWork.PaymentWay = value;
                NotifyOfPropertyChange();
            }
        }
        #endregion

        public UniversalControlVm(KeeperDataModel dataModel, BalanceDuringTransactionHinter balanceDuringTransactionHinter,
                 AccNameSelector accNameSelectionControlInitializer)
        {
            _dataModel = dataModel;
            _accNameSelectionControlInitializer = accNameSelectionControlInitializer;
            _balanceDuringTransactionHinter = balanceDuringTransactionHinter;

            PaymentWays = Enum.GetValues(typeof(PaymentWay)).OfType<PaymentWay>().ToList();
        }

        // if true should not be changed by associations
        private bool _counterpartyChangedManually; 
        private bool _categoryChangedManually;

        public void SetTran(TransactionModel tran)
        {
            _counterpartyChangedManually = false;
            _categoryChangedManually = false;

            TranInWork = tran;
            TranInWork.PropertyChanged += TranInWork_PropertyChanged;

            SelectedPaymentWay = TranInWork.PaymentWay == PaymentWay.НеЗадано
                ? PaymentGuess.GuessPaymentWay(TranInWork)
                : TranInWork.PaymentWay;

            MyAccNameSelectorVm = _accNameSelectionControlInitializer.ForMyAccount(TranInWork);
            MyAccNameSelectorVm.PropertyChanged += MyAccNameSelectorVm_PropertyChanged;

            CounterpartySelectorVm = _accNameSelectionControlInitializer.ForCounterparty(TranInWork);
            if (TranInWork.Counterparty == null)
                TranInWork.Counterparty = _dataModel.AcMoDict[CounterpartySelectorVm.MyAccName.Id];
            CounterpartySelectorVm.PropertyChanged += CounterpartySelectorVm_PropertyChanged;

            CategorySelectorVm = _accNameSelectionControlInitializer.ForCategory(TranInWork);
            if (TranInWork.Category == null)
                TranInWork.Category = _dataModel.AcMoDict[CategorySelectorVm.MyAccName.Id];
            CategorySelectorVm.PropertyChanged += CategorySelectorVm_PropertyChanged;

            MySecondAccNameSelectorVm = _accNameSelectionControlInitializer.ForMySecondAccount(TranInWork);
            MySecondAccNameSelectorVm.PropertyChanged += MySecondAccNameSelectorVm_PropertyChanged;

            MyAmountInputControlVm = new AmountInputControlVm
            {
                LabelContent = GetAmountActionLabel(TranInWork),
                AmountColor = TranInWork.Operation.FontColor(),
                Amount = TranInWork.Amount,
                Currency = TranInWork.Currency,
                ButtonAllInVisibility = tran.Operation == OperationType.Доход ? Visibility.Collapsed : Visibility.Visible,
            };
            MyAmountInputControlVm.PropertyChanged += MyAmountInputControlVm_PropertyChanged;
            MyAccountBalance = _balanceDuringTransactionHinter.GetMyAccountBalance(TranInWork);

            MyAmountInReturnInputControlVm = new AmountInputControlVm
            {
                LabelContent = "Получил",
                AmountColor = TranInWork.Operation.FontColor(),
                Amount = TranInWork.AmountInReturn,
                Currency = TranInWork.CurrencyInReturn ?? CurrencyCode.BYN,
                ButtonAllInVisibility = Visibility.Collapsed,
            };
            MyAmountInReturnInputControlVm.PropertyChanged += MyAmountInReturnInputControlVm_PropertyChanged;


            MyTagPickerVm = new TagPickerVm { TagSelectorVm = _accNameSelectionControlInitializer.ForAdditionalTags(TranInWork) };
            foreach (var tag in tran.Tags)
            {
                var alreadyChosenTag = MyTagPickerVm.TagSelectorVm.AvailableAccNames.FindThroughTheForestById(tag.Id);
                if (alreadyChosenTag != null)
                    MyTagPickerVm.Tags.Add(alreadyChosenTag);
            }
            MyTagPickerVm.Tags.CollectionChanged += Tags_CollectionChanged;

            MyDatePickerVm = new DatePickerWithTrianglesVm() { SelectedDate = TranInWork.Timestamp };
            MyDatePickerVm.PropertyChanged += MyDatePickerVm_PropertyChanged;
        }

        private string GetAmountActionLabel(TransactionModel tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход: return "Получил";
                case OperationType.Расход: return "Заплатил";
                case OperationType.Перенос: return "Перенес";
                // case OperationType.Обмен:
                default: return "Сдал";
            }
        }

        private void MyAccNameSelectorVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MyAccName")
            {
                TranInWork.MyAccount = _dataModel.AcMoDict[MyAccNameSelectorVm.MyAccName.Id];
                MyAmountInputControlVm.Currency =
                    _dataModel.Transactions.Values
                        .LastOrDefault(t => t.MyAccount.Id == TranInWork.MyAccount.Id)?.Currency ?? CurrencyCode.BYN;
                SelectedPaymentWay = PaymentGuess.GuessPaymentWay(TranInWork);
            }
        }

        private void MySecondAccNameSelectorVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MyAccName")
            {
                TranInWork.MySecondAccount = _dataModel.AcMoDict[MySecondAccNameSelectorVm.MyAccName.Id];
                MyAmountInReturnInputControlVm.Currency =
                    _dataModel.Transactions.Values
                        .LastOrDefault(t => t.MyAccount.Id == TranInWork.MySecondAccount.Id)?.Currency ?? CurrencyCode.BYN;
            }
        }

        private void CounterpartySelectorVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "MyAccName") return;
            _counterpartyChangedManually = true;

            TranInWork.Counterparty = _dataModel.AcMoDict[CounterpartySelectorVm.MyAccName.Id];
            SelectedPaymentWay = PaymentGuess.GuessPaymentWay(TranInWork);

            if (!_categoryChangedManually && IsAddMode)
            {
                var associatedCategory = FindAssociated(TranInWork.Counterparty, TranInWork.Operation);
                if (associatedCategory != null)
                {
                    TranInWork.Category = associatedCategory;
                    CategorySelectorVm = _accNameSelectionControlInitializer.ForCategory(TranInWork);
                    CategorySelectorVm.PropertyChanged += CategorySelectorVm_PropertyChanged;
                }
            }

            AddTagIfAssociated(TranInWork.Counterparty.AssociatedTagId);
        }

        private void CategorySelectorVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "MyAccName") return;
            _categoryChangedManually = true;

            TranInWork.Category = _dataModel.AcMoDict[CategorySelectorVm.MyAccName.Id];
            SelectedPaymentWay = PaymentGuess.GuessPaymentWay(TranInWork);

            if (!_counterpartyChangedManually && IsAddMode)
            {
                var associatedCounterparty = FindAssociated(TranInWork.Category, TranInWork.Operation);
                if (associatedCounterparty != null)
                {
                    TranInWork.Counterparty = associatedCounterparty;
                    CounterpartySelectorVm = _accNameSelectionControlInitializer.ForCounterparty(TranInWork);
                    CounterpartySelectorVm.PropertyChanged += CounterpartySelectorVm_PropertyChanged;
                }
            }

            AddTagIfAssociated(TranInWork.Category.AssociatedTagId);
        }

        private AccountItemModel FindAssociated(AccountItemModel account, OperationType opType)
        {

            var associatedId = account.IsCategory()
                ? account.AssociatedExternalId
                : opType == OperationType.Доход
                    ? account.AssociatedIncomeId
                    : account.AssociatedExpenseId;

            return associatedId == 0 ? null : _dataModel.AcMoDict[associatedId];
        }

        private void AddTagIfAssociated(int tagId)
        {
            if (tagId == 0) return;
            if (MyTagPickerVm.Tags.Any(t => t.Id == tagId)) return;

            var tag = _dataModel.AcMoDict[tagId];
            MyTagPickerVm.Tags.Add(new AccName().PopulateFromAccount(tag, null));
        }

        private void MyAmountInputControlVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Amount") TranInWork.Amount = MyAmountInputControlVm.Amount;
            if (e.PropertyName == "Currency") TranInWork.Currency = MyAmountInputControlVm.Currency;
            if (e.PropertyName == "ButtonAllInPressed")
            {
                MyAmountInputControlVm.Amount =
                    _dataModel.Transactions.Values.Sum(a => a.AmountForAccount(TranInWork.MyAccount, TranInWork.Currency, TranInWork.Timestamp.AddMilliseconds(-1)));
            }
        }

        private void MyAmountInReturnInputControlVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Amount") TranInWork.AmountInReturn = MyAmountInReturnInputControlVm.Amount;
            if (e.PropertyName == "Currency") TranInWork.CurrencyInReturn = MyAmountInReturnInputControlVm.Currency;
        }

        #region Tags

        private void Tags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
                ReactOnRemove();
            if (e.Action == NotifyCollectionChangedAction.Add && MyTagPickerVm.TagInWork != null)
                ReactOnUsersAdd();
        }

        private void ReactOnRemove()
        {
            var tag = _dataModel.AcMoDict[MyTagPickerVm.TagInWork.Id];
            TranInWork.Tags.Remove(tag);
            MyTagPickerVm.TagInWork = null;
        }

        private void ReactOnUsersAdd()
        {
            var tag = _dataModel.AcMoDict[MyTagPickerVm.TagInWork.Id];
            TranInWork.Tags.Add(tag);
            MyTagPickerVm.TagInWork = null;
        }

        #endregion

        private void MyDatePickerVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var selectedDate = MyDatePickerVm.SelectedDate;
            var dayTransactions = _dataModel.Transactions.Values.Where(t => t.Timestamp.Date == selectedDate.Date).ToList();

            int minute = 1;
            if (dayTransactions.Any())
                minute = dayTransactions.Max(t => t.Timestamp.Minute) + 1;

            TranInWork.Timestamp = selectedDate.Date.AddMinutes(minute);
        }

        private void TranInWork_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "MyAccount":
                    MyAccountBalance = _balanceDuringTransactionHinter.GetMyAccountBalance(TranInWork);
                    break;
                case "MySecondAccount":
                    NotifyOfPropertyChange(nameof(MySecondAccountBalance));
                    break;
                case "Operation":
                case "Amount":
                case "AmountInReturn":
                case "Currency":
                case "CurrencyInReturn":
                case "Timestamp":
                    NotifyOfPropertyChange(nameof(AmountInUsd));
                    NotifyOfPropertyChange(nameof(AmountInReturnInUsd));
                    MyAccountBalance = _balanceDuringTransactionHinter.GetMyAccountBalance(TranInWork);
                    NotifyOfPropertyChange(nameof(MySecondAccountBalance));
                    NotifyOfPropertyChange(nameof(ExchangeRate));
                    break;
            }
        }

    }
}
