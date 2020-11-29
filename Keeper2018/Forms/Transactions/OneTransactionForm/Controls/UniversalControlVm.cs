using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
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

        private readonly KeeperDb _db;
        private readonly AccNameSelectionControlInitializer _accNameSelectionControlInitializer;
        private readonly AssociationFinder _associationFinder;
        private readonly BalanceDuringTransactionHinter _balanceDuringTransactionHinter;

        private AmountInputControlVm _myAmountInputControlVm;
        private AccNameSelectorVm _mySecondAccNameSelectorVm;
        private AccNameSelectorVm _myAccNameSelectorVm;
        private AmountInputControlVm _myAmountInReturnInputControlVm;
        private TagPickerVm _myTagPickerVm;
        private DatePickerWithTrianglesVm _myDatePickerVm;

        public TransactionModel TranInWork { get; set; } = new TransactionModel();

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

        //  public string MyAccountBalance => _balanceDuringTransactionHinter.GetMyAccountBalance(TranInWork);
        private string _myAccountBalance;
        private PaymentWay _selectedPaymentWay;

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

        public UniversalControlVm(KeeperDb db, BalanceDuringTransactionHinter balanceDuringTransactionHinter,
                 AccNameSelectionControlInitializer accNameSelectionControlInitializer, AssociationFinder associationFinder)
        {
            _db = db;
            _accNameSelectionControlInitializer = accNameSelectionControlInitializer;
            _associationFinder = associationFinder;
            _balanceDuringTransactionHinter = balanceDuringTransactionHinter;

            PaymentWays = Enum.GetValues(typeof(PaymentWay)).OfType<PaymentWay>().ToList();
        }

        public void SetTran(TransactionModel tran)
        {
            TranInWork = tran;
            TranInWork.PropertyChanged += TranInWork_PropertyChanged;

            SelectedPaymentWay = TranInWork.PaymentWay == PaymentWay.НеЗадано ? GuessPaymentWay() : TranInWork.PaymentWay;

            MyAccNameSelectorVm = _accNameSelectionControlInitializer.ForMyAccount(TranInWork);
            MyAccNameSelectorVm.PropertyChanged += MyAccNameSelectorVm_PropertyChanged;

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
            MyAmountInputControlVm.PropertyChanged += MyAmountInputcControlVm_PropertyChanged;
            Task.Factory.StartNew(S);

            MyAmountInReturnInputControlVm = new AmountInputControlVm
            {
                LabelContent = "Получил",
                AmountColor = TranInWork.Operation.FontColor(),
                Amount = TranInWork.AmountInReturn,
                Currency = TranInWork.CurrencyInReturn ?? CurrencyCode.BYN,
                ButtonAllInVisibility = Visibility.Collapsed,
            };
            MyAmountInReturnInputControlVm.PropertyChanged += MyAmountInReturnInputControlVm_PropertyChanged;


            MyTagPickerVm = new TagPickerVm { TagSelectorVm = _accNameSelectionControlInitializer.ForTags(TranInWork) };
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
        private void Tags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove) ReactOnRemove();
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (MyTagPickerVm.TagInWork != null)
                    ReactOnUsersAdd();
                else ReactOnAssociationAdd();
            }

            SelectedPaymentWay = GuessPaymentWay();
        }

        private void ReactOnUsersAdd()
        {
            var tag = _db.AcMoDict[MyTagPickerVm.TagInWork.Id];
            TranInWork.Tags.Add(tag);

            var associatedTag = _associationFinder.GetAssociation(TranInWork, tag);
            if (associatedTag != null)
            {
                MyTagPickerVm.AssociatedTag = new AccName().PopulateFromAccount(associatedTag, null);
            }

            MyTagPickerVm.TagInWork = null;
        }

        private void ReactOnAssociationAdd()
        {
            var tag = _db.AcMoDict[MyTagPickerVm.AssociatedTag.Id];
            TranInWork.Tags.Add(tag);

            MyTagPickerVm.AssociatedTag = null;
        }

        private void ReactOnRemove()
        {
            var tag = _db.AcMoDict[MyTagPickerVm.TagInWork.Id];
            TranInWork.Tags.Remove(tag);
            MyTagPickerVm.TagInWork = null;
        }

        private void MyDatePickerVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var selectedDate = MyDatePickerVm.SelectedDate;
            var dayTransactions = _db.Bin.Transactions.Values.Where(t => t.Timestamp.Date == selectedDate.Date).ToList();

            int minute = 1;
            if (dayTransactions.Any())
                minute = dayTransactions.Max(t => t.Timestamp.Minute) + 1;


            TranInWork.Timestamp = selectedDate.Date.AddMinutes(minute);
        }

        private void MyAmountInputcControlVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Amount") TranInWork.Amount = MyAmountInputControlVm.Amount;
            if (e.PropertyName == "Currency") TranInWork.Currency = MyAmountInputControlVm.Currency;
            if (e.PropertyName == "ButtonAllInPressed")
            {
                MyAmountInputControlVm.Amount =
                    _db.Bin.Transactions.Values.Sum(a => a.AmountForAccount(_db, TranInWork.MyAccount, TranInWork.Currency, TranInWork.Timestamp.AddMilliseconds(-1)));
            }
        }

        private void MyAmountInReturnInputControlVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Amount") TranInWork.AmountInReturn = MyAmountInReturnInputControlVm.Amount;
            if (e.PropertyName == "Currency") TranInWork.CurrencyInReturn = MyAmountInReturnInputControlVm.Currency;
        }

        private void MyAccNameSelectorVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MyAccName")
            {
                TranInWork.MyAccount = _db.AcMoDict[MyAccNameSelectorVm.MyAccName.Id];
                MyAmountInputControlVm.Currency =
                    _db.Bin.Transactions.Values.LastOrDefault(t => t.MyAccount == TranInWork.MyAccount.Id)?.Currency ?? CurrencyCode.BYN;
                SelectedPaymentWay = GuessPaymentWay();
            }
        }

        private static List<int> ERIP = new List<int>
        {
            192, 363, 285,  // коммунальные квартира и дача, кредит
            270, 736, 303, 183, // велком, школьная столовая, страховщики, государство
        }; 
        private static List<int> Terminal = new List<int> { 179, 353 }; // магазины, медицина
        private static List<int> CardOther = new List<int> { 718, 264 }; // заправка, минтранс

        private PaymentWay GuessPaymentWay()
        {
            if (TranInWork.Operation != OperationType.Расход)
                return PaymentWay.НеЗадано;
            if (TranInWork.MyAccount.Is(160))
                return PaymentWay.Наличные;
            if (TranInWork.MyAccount.Is(161))
            {
                foreach (var tag in TranInWork.Tags)
                {
                    foreach (var grou in Terminal)
                    {
                        if (tag.Is(grou))
                            return PaymentWay.КартаТерминал;
                    }
                    foreach (var grou in CardOther)
                    {
                        if (tag.Is(grou))
                            return PaymentWay.КартаДругое;
                    }
                    foreach (var grou in ERIP)
                    {
                        if (tag.Is(grou))
                            return PaymentWay.КартаЕрип;
                    }
                }
                return PaymentWay.НеЗадано;
            }
            return PaymentWay.НеЗадано;
        }

        private void MySecondAccNameSelectorVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MyAccName")
            {
                TranInWork.MySecondAccount = _db.AcMoDict[MySecondAccNameSelectorVm.MyAccName.Id];
                MyAmountInReturnInputControlVm.Currency =
                    _db.Bin.Transactions.Values.LastOrDefault(t => t.MyAccount == TranInWork.MySecondAccount.Id)?.Currency ?? CurrencyCode.BYN;
            }
        }

        private void TranInWork_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "MyAccount":
                    Task.Factory.StartNew(S);
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
                    Task.Factory.StartNew(S);
                    NotifyOfPropertyChange(nameof(MySecondAccountBalance));
                    NotifyOfPropertyChange(nameof(ExchangeRate));
                    break;
            }
        }

        private void S()
        {
            var result = _balanceDuringTransactionHinter.GetMyAccountBalance(TranInWork);
            if (Application.Current.Dispatcher != null)
                Application.Current.Dispatcher.Invoke(() => MyAccountBalance = result);
        }
    }
}
