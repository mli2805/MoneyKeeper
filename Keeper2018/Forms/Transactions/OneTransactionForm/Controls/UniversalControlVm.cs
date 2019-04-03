using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;

namespace Keeper2018
{
    public class UniversalControlVm : PropertyChangedBase
    {
        private Visibility _visibility;
        public Visibility Visibility
        {
            get { return _visibility; }
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
            get { return _myAccNameSelectorVm; }
            set
            {
                if (Equals(value, _myAccNameSelectorVm)) return;
                _myAccNameSelectorVm = value;
                NotifyOfPropertyChange();
            }
        }
        public AccNameSelectorVm MySecondAccNameSelectorVm
        {
            get { return _mySecondAccNameSelectorVm; }
            set
            {
                if (Equals(value, _mySecondAccNameSelectorVm)) return;
                _mySecondAccNameSelectorVm = value;
                NotifyOfPropertyChange();
            }
        }
        public AmountInputControlVm MyAmountInputControlVm
        {
            get { return _myAmountInputControlVm; }
            set
            {
                if (Equals(value, _myAmountInputControlVm)) return;
                _myAmountInputControlVm = value;
                NotifyOfPropertyChange();
            }
        }
        public AmountInputControlVm MyAmountInReturnInputControlVm
        {
            get { return _myAmountInReturnInputControlVm; }
            set
            {
                if (Equals(value, _myAmountInReturnInputControlVm)) return;
                _myAmountInReturnInputControlVm = value;
                NotifyOfPropertyChange();
            }
        }

        public TagPickerVm MyTagPickerVm
        {
            get { return _myTagPickerVm; }
            set
            {
                if (Equals(value, _myTagPickerVm)) return;
                _myTagPickerVm = value;
                NotifyOfPropertyChange();
            }
        }
        public DatePickerWithTrianglesVm MyDatePickerVm
        {
            get { return _myDatePickerVm; }
            set
            {
                if (Equals(value, _myDatePickerVm)) return;
                _myDatePickerVm = value;
                NotifyOfPropertyChange();
            }
        }

       //  public string MyAccountBalance => _balanceDuringTransactionHinter.GetMyAccountBalance(TranInWork);
        private string _myAccountBalance;
        public string MyAccountBalance
        {
            get { return _myAccountBalance; }
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

        private bool _receiptButtonPressed;

        public bool ReceiptButtonPressed
        {
            get { return _receiptButtonPressed; }
            set
            {
                if (value == _receiptButtonPressed) return;
                _receiptButtonPressed = value;
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
        }

        public void SetTran(TransactionModel tran)
        {
            TranInWork = tran;
            TranInWork.PropertyChanged += TranInWork_PropertyChanged;

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
        }

        private void ReactOnUsersAdd()
        {
            var tag = _db.SeekAccountById(MyTagPickerVm.TagInWork.Id);
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
            var tag = _db.SeekAccountById(MyTagPickerVm.AssociatedTag.Id);
            TranInWork.Tags.Add(tag);

            MyTagPickerVm.AssociatedTag = null;
        }

        private void ReactOnRemove()
        {
            var tag = _db.SeekAccountById(MyTagPickerVm.TagInWork.Id);
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
                TranInWork.MyAccount = _db.SeekAccountById(MyAccNameSelectorVm.MyAccName.Id);
                MyAmountInputControlVm.Currency =
                    _db.Bin.Transactions.Values.LastOrDefault(t => t.MyAccount == TranInWork.MyAccount.Id)?.Currency ?? CurrencyCode.BYN;
            }
        }
        private void MySecondAccNameSelectorVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MyAccName")
            {
                TranInWork.MySecondAccount = _db.SeekAccountById(MySecondAccNameSelectorVm.MyAccName.Id);
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
                  //  NotifyOfPropertyChange(nameof(MyAccountBalance));
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
                //    NotifyOfPropertyChange(nameof(MyAccountBalance));
                    Task.Factory.StartNew(S);
                    NotifyOfPropertyChange(nameof(MySecondAccountBalance));
                    NotifyOfPropertyChange(nameof(ExchangeRate));
                    break;
            }
        }

        private void S()
        {
            var result = _balanceDuringTransactionHinter.GetMyAccountBalance(TranInWork);
            Application.Current.Dispatcher.Invoke(() => MyAccountBalance = result);
        }

    }
}
