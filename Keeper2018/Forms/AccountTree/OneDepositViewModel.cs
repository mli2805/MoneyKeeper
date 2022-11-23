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
        private AccountModel _accountModel;
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

        public OneDepositViewModel(KeeperDataModel dataModel, IWindowManager windowManager)
        {
            _dataModel = dataModel;
            _windowManager = windowManager;
        }

        public void InitializeForm(AccountModel accountModel, bool isInAddMode)
        {
            IsSavePressed = false;
            _isInAddMode = isInAddMode;
            _accountModel = accountModel;
            DepositOffers = _dataModel.DepositOffers;
            DepositInWork = accountModel.Deposit;
            DepositInWork.MyAccountId = accountModel.Id;
            ParentName = accountModel.Owner.Name;

            if (isInAddMode)
            {
                SelectedDepositOffer = DepositOffers.First();
                DepositInWork.StartDate = DateTime.Today;
                DepositInWork.FinishDate = DateTime.Today.AddMonths(1);
                Junction = "";
            }
            else
            {
                _selectedDepositOffer = DepositOffers.First(o => o.Id == DepositInWork.DepositOfferId);
                Junction = _accountModel.Name;
            }
        }

     
        protected override void OnViewLoaded(object view)
        {
            DisplayName = _isInAddMode ? "Добавить" : "Изменить";
        }

        public void PayCard()
        {
            var vm = new OneCardViewModel();
            vm.InitializeForm(_accountModel, DepositInWork.Card == null);
            _windowManager.ShowDialog(vm);
            if (vm.IsSavePressed)
            {
                DepositInWork.Card = vm.CardInWork;
                DepositInWork.Card.Id = _dataModel.AcMoDict.Values
                    .Where(a => a.IsDeposit && a.Deposit.Card != null).Max(a => a.Deposit.Card.Id) + 1;
            }
        }

        public void SaveDeposit()
        {
            IsSavePressed = true;
            if (string.IsNullOrEmpty(Junction))
                CompileAccountName();
            _accountModel.Header = Junction;
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
            decimal rate;
            var conditionses = SelectedDepositOffer.CondsMap.LastOrDefault(p => p.Key <= DepositInWork.StartDate);
            {
                var line = conditionses.Value.RateLines.Last();
                rate = line.Rate;
            }

            var startDate = DepositInWork.StartDate.ToString("d/MM/yyyy", CultureInfo.InvariantCulture);
            var finishDate = DepositInWork.FinishDate.ToString("d/MM/yyyy", CultureInfo.InvariantCulture);
            Junction = $"{SelectedDepositOffer.Bank.Header} {SelectedDepositOffer.Title} {startDate} - {finishDate} {rate:0.#}%";
        }

        public void FillDepositRatesTable()
        {
            var vm = new RulesAndRatesViewModel();
            vm.Initialize("",
                SelectedDepositOffer.CondsMap.OrderBy(k => k.Key)
                    .LastOrDefault(p => p.Key <= DepositInWork.StartDate).Value, 
                SelectedDepositOffer.RateType, _dataModel.GetDepoRateLinesMaxId());
            _windowManager.ShowDialog(vm);
        }
    }
}
