using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class OneDepositViewModel : Screen
    {
        private bool _isInAddMode;
        public AccountItemModel AccountItemModel { get; set; }
        public BankAccountModel BankAccountInWork { get; set; }
        public string ParentName { get; set; }

        private string _junction;
        public string Junction
        {
            get => _junction;
            set
            {
                if (value == _junction) return;
                _junction = value;
                NotifyOfPropertyChange();
            }
        }

        private readonly KeeperDataModel _dataModel;
        public bool IsSavePressed { get; set; }

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
                NotifyOfPropertyChange(IsNotRevocable);
            }
        }

        public string IsNotRevocable => _selectedDepositOffer.IsNotRevocable ? "Безотзывный" : "Отзывный";

        public OneDepositViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Initialize(AccountItemModel accountItemModel, bool isInAddMode)
        {
            IsSavePressed = false;
            _isInAddMode = isInAddMode;
            AccountItemModel = accountItemModel;
            DepositOffers = _dataModel.DepositOffers;
            BankAccountInWork = accountItemModel.BankAccount;
            ParentName = accountItemModel.Parent.Name;

            if (isInAddMode)
            {
                SelectedDepositOffer = DepositOffers.Last();
                Junction = "";
            }
            else
            {
                _selectedDepositOffer = DepositOffers.First(o => o.Id == BankAccountInWork.DepositOfferId);
                Junction = AccountItemModel.Name;
            }
        }

        protected override void OnViewLoaded(object view)
        {
            var cap = _isInAddMode ? "Добавить депозит" : "Изменить депозит";
            DisplayName = $"{cap} (id = {AccountItemModel.Id})";
        }

        public void SaveDeposit()
        {
            IsSavePressed = true;
            if (string.IsNullOrEmpty(Junction))
                CompileAccountName();
            AccountItemModel.Name = Junction;
            BankAccountInWork.DepositOfferId = SelectedDepositOffer.Id;
            TryClose();
        }

        public void Cancel()
        {
            IsSavePressed = false;
            TryClose();
        }

        public void CompileAccountName()
        {
            decimal rate = 0;

            if (SelectedDepositOffer.CondsMap.Count > 0)
            {
                var conditions = SelectedDepositOffer
                    .CondsMap.LastOrDefault(p => p.Key <= BankAccountInWork.StartDate);
                var line = conditions.Value?.RateLines?.LastOrDefault();
                rate = line?.Rate ?? 0;
            }

            var startDate = BankAccountInWork.StartDate.ToString("d/MM/yyyy", CultureInfo.InvariantCulture);
            var finishDate = BankAccountInWork.FinishDate.ToString("d/MM/yyyy", CultureInfo.InvariantCulture);
            Junction = $"{SelectedDepositOffer.Bank.Name} {SelectedDepositOffer.Title} {startDate} - {finishDate} {rate:0.#}%";
        }

        // public void FillDepositRatesTable()
        // {
        //     _rulesAndRatesViewModel.Initialize("",
        //         SelectedDepositOffer.CondsMap.OrderBy(k => k.Key)
        //             .LastOrDefault(p => p.Key <= BankAccountInWork.StartDate).Value,
        //         SelectedDepositOffer.RateType, _dataModel.GetDepoRateLinesMaxId());
        //     _windowManager.ShowDialog(_rulesAndRatesViewModel);
        // }
    }
}
