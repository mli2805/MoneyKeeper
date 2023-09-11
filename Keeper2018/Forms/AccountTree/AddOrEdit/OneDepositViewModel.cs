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
        public Deposit DepositInWork { get; set; }
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
        private readonly IWindowManager _windowManager;
        private readonly RulesAndRatesViewModel _rulesAndRatesViewModel;
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
                DepositInWork.DepositOfferId = _selectedDepositOffer.Id;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(IsNotRevocable);
            }
        }

        public string IsNotRevocable => _selectedDepositOffer.IsNotRevocable ? "Безотзывный" : "Отзывный";

        public OneDepositViewModel(KeeperDataModel dataModel, IWindowManager windowManager, 
            RulesAndRatesViewModel rulesAndRatesViewModel)
        {
            _dataModel = dataModel;
            _windowManager = windowManager;
            _rulesAndRatesViewModel = rulesAndRatesViewModel;
        }

        public void Initialize(AccountItemModel accountItemModel, bool isInAddMode)
        {
            IsSavePressed = false;
            _isInAddMode = isInAddMode;
            AccountItemModel = accountItemModel;
            DepositOffers = _dataModel.DepositOffers;
            DepositInWork = accountItemModel.Deposit;
            DepositInWork.MyAccountId = accountItemModel.Id;
            ParentName = accountItemModel.Parent.Name;

            if (isInAddMode)
            {
                SelectedDepositOffer = DepositOffers.Last();
                DepositInWork.StartDate = DateTime.Today;
                DepositInWork.FinishDate = DateTime.Today.AddMonths(1);
                Junction = "";
            }
            else
            {
                _selectedDepositOffer = DepositOffers.First(o => o.Id == DepositInWork.DepositOfferId);
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
            DepositInWork.DepositOfferId = SelectedDepositOffer.Id;
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
                    .CondsMap.LastOrDefault(p => p.Key <= DepositInWork.StartDate);
                var line = conditions.Value?.RateLines?.LastOrDefault();
                rate = line?.Rate ?? 0;
            }

            var startDate = DepositInWork.StartDate.ToString("d/MM/yyyy", CultureInfo.InvariantCulture);
            var finishDate = DepositInWork.FinishDate.ToString("d/MM/yyyy", CultureInfo.InvariantCulture);
            Junction = $"{SelectedDepositOffer.Bank.Name} {SelectedDepositOffer.Title} {startDate} - {finishDate} {rate:0.#}%";
        }

        public void FillDepositRatesTable()
        {
            _rulesAndRatesViewModel.Initialize("",
                SelectedDepositOffer.CondsMap.OrderBy(k => k.Key)
                    .LastOrDefault(p => p.Key <= DepositInWork.StartDate).Value,
                SelectedDepositOffer.RateType, _dataModel.GetDepoRateLinesMaxId());
            _windowManager.ShowDialog(_rulesAndRatesViewModel);
        }
    }
}
