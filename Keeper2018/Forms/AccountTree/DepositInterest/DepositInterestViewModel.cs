using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class DepositInterestViewModel : Screen
    {
        private readonly KeeperDataModel _keeperDataModel;
        private readonly IWindowManager _windowManager;
        private readonly ComboTreesProvider _comboTreesProvider;
        private readonly ShellPartsBinder _shellPartsBinder;
        private readonly AccNameSelector _accNameSelectionControlInitializer;
        private AccountItemModel _accountItemModel;
        private DepositOfferModel _depositOffer;

        public string BankTitle { get; set; }

        public bool IsPercent { get; set; }
        public bool IsMoneyBack { get; set; }

        public string DepositTitle { get; set; }

        private decimal _depositBalance;
        private decimal _myNextAccountBalance;
        public string MyNextAccountBalanceStr =>
           $"{_myNextAccountBalance:#,0.00} {DepositCurrency} -> {_myNextAccountBalance + Amount:#,0.00} {DepositCurrency}";
        public string DepositBalanceStr => $"{_depositBalance:#,0.00} {DepositCurrency} -> {_depositBalance + Amount:#,0.00} {DepositCurrency}";

        public string DepositCurrency { get; set; }

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set
            {
                if (value == _amount) return;
                _amount = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(DepositBalanceStr));
                NotifyOfPropertyChange(nameof(MyNextAccountBalanceStr));
            }
        }

        public AccNameSelectorVm MyNextAccNameSelectorVm { get; set; }

        private bool _isTransferred;

        public bool IsTransferred
        {
            get => _isTransferred;
            set
            {
                if (value == _isTransferred) return;
                _isTransferred = value;
                NotifyOfPropertyChange();
            }
        }

        private DateTime _transactionTimestamp;
        public DatePickerWithTrianglesVm MyDatePickerVm { get; set; }
        public string Comment { get; set; } = "";

        public DepositInterestViewModel(KeeperDataModel keeperDataModel, IWindowManager windowManager,
            ComboTreesProvider comboTreesProvider, ShellPartsBinder shellPartsBinder,
            AccNameSelector accNameSelectionControlInitializer)
        {
            _keeperDataModel = keeperDataModel;
            _windowManager = windowManager;
            _comboTreesProvider = comboTreesProvider;
            _shellPartsBinder = shellPartsBinder;
            _accNameSelectionControlInitializer = accNameSelectionControlInitializer;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Начислены проценты/кэшбек";
        }

        public bool Initialize(AccountItemModel accountItemModel)
        {
            _accountItemModel = accountItemModel;
            var depositOfferId = accountItemModel.IsDeposit ? accountItemModel.Deposit.DepositOfferId : accountItemModel.PayCard.DepositOfferId;
            _depositOffer = _keeperDataModel.DepositOffers.First(o => o.Id == depositOfferId);
            BankTitle = _depositOffer.Bank.Name;
            IsMoneyBack = _accountItemModel.IsCard;
            IsPercent = !IsMoneyBack;
            DepositTitle = accountItemModel.Name;
            DepositCurrency = _depositOffer.MainCurrency.ToString().ToUpper();
            _comboTreesProvider.Initialize();
            Amount = 0;
            MyNextAccNameSelectorVm = _accNameSelectionControlInitializer.ForMyNextAccount();
            MyNextAccNameSelectorVm.PropertyChanged += MyNextAccNameSelectorVm_PropertyChanged;

            MyDatePickerVm = new DatePickerWithTrianglesVm() { SelectedDate = DateTime.Today };
            _transactionTimestamp = DateTime.Today.AddDays(1).AddMilliseconds(-1);
            MyDatePickerVm.PropertyChanged += MyDatePickerVm_PropertyChanged;

            _depositBalance = _keeperDataModel.Transactions.Values.Sum(t => t.AmountForAccount(
                _accountItemModel, _depositOffer.MainCurrency, _transactionTimestamp));

            return true;
        }

        private void MyNextAccNameSelectorVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MyAccName")
            {
                var nextAccountModel = _keeperDataModel.AcMoDict[MyNextAccNameSelectorVm.MyAccName.Id];
                _myNextAccountBalance = _keeperDataModel.Transactions.Values.Sum(t => t.AmountForAccount(
                    nextAccountModel, _depositOffer.MainCurrency, _transactionTimestamp));
                NotifyOfPropertyChange(nameof(MyNextAccountBalanceStr));
            }
        }

        private void MyDatePickerVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var selectedDate = MyDatePickerVm.SelectedDate;
            var dayTransactions = _keeperDataModel.Transactions.Values.Where(t => t.Timestamp.Date == selectedDate.Date).ToList();

            int minute = 1;
            if (dayTransactions.Any())
                minute = dayTransactions.Max(t => t.Timestamp.Minute) + 1;

            _transactionTimestamp = selectedDate.Date.AddMinutes(minute);

            _depositBalance = _keeperDataModel.Transactions.Values.Sum(t => t.AmountForAccount(
                _accountItemModel, _depositOffer.MainCurrency, _transactionTimestamp));
            var nextAccountModel = _keeperDataModel.AcMoDict[MyNextAccNameSelectorVm.MyAccName.Id];
            _myNextAccountBalance = _keeperDataModel.Transactions.Values.Sum(t => t.AmountForAccount(
                nextAccountModel, _depositOffer.MainCurrency, _transactionTimestamp));

            NotifyOfPropertyChange(nameof(DepositBalanceStr));
            NotifyOfPropertyChange(nameof(MyNextAccountBalanceStr));
        }

        public void Save()
        {
            AccountItemModel nextAccountItemModel = null;
            if (IsTransferred)
            {
                nextAccountItemModel = _keeperDataModel.AcMoDict[MyNextAccNameSelectorVm.MyAccName.Id];
                if (_accountItemModel.Id == nextAccountItemModel.Id)
                {
                    var vm = new MyMessageBoxViewModel(MessageType.Error, "Перечисление на самого себя!");
                    _windowManager.ShowDialog(vm);
                    return;
                }
            }

            var id = _keeperDataModel.Transactions.Keys.Max() + 1;
            var thisDateTrans = _keeperDataModel.Transactions.Values
                .Where(t => t.Timestamp.Date == MyDatePickerVm.SelectedDate)
                .OrderBy(l => l.Timestamp)
                .LastOrDefault();
            var timestamp = thisDateTrans?.Timestamp ?? MyDatePickerVm.SelectedDate;
            var tranModel1 = new TransactionModel()
            {
                Id = id,
                Timestamp = timestamp.AddMinutes(1),
                Operation = OperationType.Доход,
                MyAccount = _accountItemModel,
                Amount = Amount,
                Currency = _depositOffer.MainCurrency,
                Tags = new List<AccountItemModel>() { _depositOffer.Bank,  },
                Comment = Comment,
            };
            tranModel1.Tags.Add(IsPercent ? _keeperDataModel.AcMoDict[208] : _keeperDataModel.AcMoDict[701]);
            _keeperDataModel.Transactions.Add(tranModel1.Id, tranModel1);

            if (IsTransferred)
            {
                var tranModel2 = new TransactionModel()
                {
                    Id = id + 1,
                    Timestamp = timestamp.AddMinutes(2),
                    Operation = OperationType.Перенос,
                    MyAccount = _accountItemModel,
                    MySecondAccount = nextAccountItemModel,
                    Amount = Amount,
                    Currency = _depositOffer.MainCurrency,
                    Tags = new List<AccountItemModel>(),
                    Comment = "",
                };
                _keeperDataModel.Transactions.Add(tranModel2.Id, tranModel2);
            }

            _shellPartsBinder.JustToForceBalanceRecalculation = DateTime.Now;
      
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}
