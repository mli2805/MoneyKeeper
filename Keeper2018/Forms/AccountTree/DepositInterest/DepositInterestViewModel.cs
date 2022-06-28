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
        private readonly ComboTreesProvider _comboTreesProvider;
        private readonly AccNameSelectionControlInitializer _accNameSelectionControlInitializer;
        private AccountModel _accountModel;
        private DepositOfferModel _depositOffer;

        public string BankTitle { get; set; }
        public string DepositTitle { get; set; }
        public string DepositCurrency { get; set; }
        public decimal Amount { get; set; }
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

        public DatePickerWithTrianglesVm MyDatePickerVm { get; set; }
        public string Comment { get; set; } = "";

        public DepositInterestViewModel(KeeperDataModel keeperDataModel, ComboTreesProvider comboTreesProvider,
            AccNameSelectionControlInitializer accNameSelectionControlInitializer)
        {
            _keeperDataModel = keeperDataModel;
            _comboTreesProvider = comboTreesProvider;
            _accNameSelectionControlInitializer = accNameSelectionControlInitializer;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Начислены проценты";
        }

        public bool Initialize(AccountModel accountModel)
        {
            if (!accountModel.IsDeposit)
                return false;

            _accountModel = accountModel;
            var depo = accountModel.Deposit;
            _depositOffer = _keeperDataModel.DepositOffers.First(o => o.Id == depo.DepositOfferId);
            BankTitle = _depositOffer.Bank.Name;
            DepositTitle = accountModel.Name;
            DepositCurrency = _depositOffer.MainCurrency.ToString().ToUpper();
            _comboTreesProvider.Initialize();
            MyNextAccNameSelectorVm = _accNameSelectionControlInitializer.ForMyNextAccount();
            MyDatePickerVm = new DatePickerWithTrianglesVm() { SelectedDate = DateTime.Today };

            return true;
        }

        public void Save()
        {
            var id = _keeperDataModel.Transactions.Keys.Max() + 1;
            var timestamp = _keeperDataModel.Transactions.Values.Where(t => t.Timestamp.Date == MyDatePickerVm.SelectedDate).Max(d => d.Timestamp);
            var tranModel1 = new TransactionModel()
            {
                Id = id,
                Timestamp = timestamp.AddMinutes(1),
                Operation = OperationType.Доход,
                MyAccount = _accountModel,
                Amount = Amount,
                Currency = _depositOffer.MainCurrency,
                Tags = new List<AccountModel>() { _depositOffer.Bank },
                Comment = Comment,
            };
            _keeperDataModel.Transactions.Add(tranModel1.Id, tranModel1);

            if (IsTransferred)
            {
                var nextAccountModel = _keeperDataModel.AcMoDict[MyNextAccNameSelectorVm.MyAccName.Id];
                var tranModel2 = new TransactionModel()
                {
                    Id = id + 1,
                    Timestamp = timestamp.AddMinutes(2),
                    Operation = OperationType.Перенос,
                    MyAccount = _accountModel,
                    MySecondAccount = nextAccountModel,
                    Amount = Amount,
                    Currency = _depositOffer.MainCurrency,
                    Tags = new List<AccountModel>(),
                    Comment = "",
                };
                _keeperDataModel.Transactions.Add(tranModel2.Id, tranModel2);
            }

            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}
