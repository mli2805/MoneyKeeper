using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using KeeperDomain;
using Microsoft.EntityFrameworkCore.Storage;

namespace Keeper2018
{
    public class OneBankAccountViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private bool _isInAddMode;
        public AccountItemModel AccountItemModel { get; set; }
        public BankAccountModel BankAccountInWork { get; set; }

        public List<AccountItemModel> Banks { get; set; }
        public List<string> BankNames { get; set; }

        private string _selectedBankName;
        public string SelectedBankName
        {
            get => _selectedBankName;
            set
            {
                if (value == _selectedBankName) return;
                _selectedBankName = value;
                BankAccountInWork.BankId = Banks.First(b => b.Name == _selectedBankName).Id;
                DepositOffers = _dataModel.DepositOffers.Where(o => o.Bank.Name == _selectedBankName).ToList();
                NotifyOfPropertyChange();
            }
        }

        private List<DepositOfferModel> _depositOffers;
        public List<DepositOfferModel> DepositOffers
        {
            get => _depositOffers;
            set
            {
                if (Equals(value, _depositOffers)) return;
                _depositOffers = value;
                NotifyOfPropertyChange();
            }
        }

        private DepositOfferModel _selectedDepositOffer;

        public DepositOfferModel SelectedDepositOffer
        {
            get => _selectedDepositOffer;
            set
            {
                if (Equals(value, _selectedDepositOffer) || value == null) return;
                _selectedDepositOffer = value;
                BankAccountInWork.BankId = _selectedDepositOffer.Bank.Id;
                BankAccountInWork.DepositOfferId =_selectedDepositOffer.Id;
                BankAccountInWork.MainCurrency = _selectedDepositOffer.MainCurrency;
                BankAccountInWork.StartDate = DateTime.Today;
                BankAccountInWork.FinishDate = _selectedDepositOffer.DepositTerm.AddTo(BankAccountInWork.StartDate).GetEndOfMonth();
                NotifyOfPropertyChange();
            }
        }

        public string ParentName { get; set; }
        public string AccountName { get; set; }

        public bool IsSavePressed { get; set; }

        public List<PaymentSystem> PaymentSystems { get; set; }
        public Visibility PayCardSectionVisibility { get; set; }

        public OneBankAccountViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Initialize(AccountItemModel accountItemModel, bool isInAddMode, bool isCard)
        {
            IsSavePressed = false;
            PayCardSectionVisibility = isCard ? Visibility.Visible : Visibility.Collapsed;
            _isInAddMode = isInAddMode;

            var folder = accountItemModel.Parent.Name;

            Banks = _dataModel.AcMoDict[220].Children.Select(c=>(AccountItemModel)c).ToList();
            BankNames = Banks.Select(b => b.Name).ToList();


            AccountItemModel = accountItemModel;
            BankAccountInWork = AccountItemModel.BankAccount;
            ParentName = accountItemModel.Parent.Name;
            PaymentSystems = Enum.GetValues(typeof(PaymentSystem)).Cast<PaymentSystem>().ToList();

            if (isInAddMode)
            {
                BankAccountInWork.IsMine = true;
                if (isCard)
                    BankAccountInWork.PayCard.CardHolder = "LEANID MARHOLIN";

                SelectedBankName = BankNames.FirstOrDefault(b=>b == folder) ?? BankNames.First();
                DepositOffers = _dataModel.DepositOffers.Where(o => o.Bank.Name == SelectedBankName).ToList();
                SelectedDepositOffer = DepositOffers.LastOrDefault();
            }
            else
            {
                AccountName = AccountItemModel.Name;

                var bank = _dataModel.AcMoDict[accountItemModel.BankAccount.BankId];
                SelectedBankName = bank.Name;
                DepositOffers = _dataModel.DepositOffers.Where(o => o.Bank.Name == SelectedBankName).ToList();
                _selectedDepositOffer = DepositOffers.FirstOrDefault(o => o.Id == BankAccountInWork.DepositOfferId);
            }
        }

        protected override void OnViewLoaded(object view)
        {
            var cap = _isInAddMode ? "Добавить счет в банке" : "Изменить счет в банке";
            DisplayName = $"{cap} (id = {AccountItemModel.Id})";
        }

        public void Save()
        {
            IsSavePressed = true;

            AccountItemModel.Name = string.IsNullOrEmpty(AccountName) ? "Без имени" : AccountName;
            TryClose();
        }

        public void Cancel()
        {
            IsSavePressed = false;
            TryClose();
        }

    }
}
