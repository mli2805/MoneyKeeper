﻿using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class OneCardViewModel : Screen
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

        public List<PaymentSystem> PaymentSystems { get; set; }
        public bool IsSavePressed { get; set; }

        public OneCardViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Initialize(AccountItemModel accountItemModel, bool isInAddMode)
        {
            IsSavePressed = false;
            _isInAddMode = isInAddMode;

            var folder = accountItemModel.Parent.Name;

            Banks = _dataModel.AcMoDict[220].Children.Select(c=>(AccountItemModel)c).ToList();
            BankNames = Banks.Select(b => b.Name).ToList();


            AccountItemModel = accountItemModel;
            DepositOffers = _dataModel.DepositOffers;
            BankAccountInWork = AccountItemModel.BankAccount;
            PaymentSystems = Enum.GetValues(typeof(PaymentSystem)).OfType<PaymentSystem>().ToList();
            ParentName = accountItemModel.Parent.Name;

            if (isInAddMode)
            {
                BankAccountInWork.PayCard.CardHolder = "LEANID MARHOLIN";
                BankAccountInWork.IsMine = true;

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
            var cap = _isInAddMode ? "Добавить карточку" : "Изменить карточку";
            DisplayName = $"{cap} (id = {AccountItemModel.Id})";
        }

        public void SaveCard()
        {
            IsSavePressed = true;

            AccountItemModel.Name = string.IsNullOrEmpty(AccountName) ? "Безымянная" : AccountName;
            BankAccountInWork.DepositOfferId = SelectedDepositOffer.Id;
            TryClose();
        }

        public void Cancel()
        {
            IsSavePressed = false;
            TryClose();
        }

    }
}
