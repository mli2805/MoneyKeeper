using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class OneDepositViewModel : Screen
    {
        private bool _isInAddMode;
        private AccountModel _accountModel;
        public Deposit DepositInWork { get; set; }
        public string ParentName { get; set; }

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

        private readonly KeeperDb _db;
        private readonly IWindowManager _windowManager;
        public bool IsSavePressed { get; set; }

        private List<DepositOfferModel> _depositOffers;

        public List<DepositOfferModel> DepositOffers
        {
            get { return _depositOffers; }
            set
            {
                if (Equals(value, _depositOffers)) return;
                _depositOffers = value;
                NotifyOfPropertyChange();
            }
        }

        private DepositOfferModel _selectedDepositOffer;
        private string _junction;

        public DepositOfferModel SelectedDepositOffer
        {
            get { return _selectedDepositOffer; }
            set
            {
                if (Equals(value, _selectedDepositOffer)) return;
                _selectedDepositOffer = value;
                DepositInWork.DepositOfferId = _selectedDepositOffer.Id;
                NotifyOfPropertyChange();
            }
        }

        public OneDepositViewModel(KeeperDb db, IWindowManager windowManager)
        {
            _db = db;
            _windowManager = windowManager;
        }

        public void InitializeForm(AccountModel accountModel, bool isInAddMode)
        {
            _isInAddMode = isInAddMode;
            _accountModel = accountModel;
            DepositOffers = _db.Bin.DepositOffers.Select(x => x.Map(_db.Bin.AccountPlaneList)).ToList();
            DepositInWork = accountModel.Deposit;
            ParentName = accountModel.Owner.Name;

            if (isInAddMode)
            {
                SelectedDepositOffer = DepositOffers.First();
                DepositInWork.StartDate = DateTime.Today;
                DepositInWork.FinishDate = DateTime.Today.AddMonths(1);
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
            TryClose();
        }

        public void CompileAccountName()
        {
            decimal rate;
            var essentials = SelectedDepositOffer.Essentials.LastOrDefault(p => p.Key < DepositInWork.StartDate);
       //     if (essentials != null)
            {
                var line = essentials.Value.RateLines.Last();
                rate = line.Rate;
            }

            var startDate = DepositInWork.StartDate.ToString("d/MM/yyyy", CultureInfo.InvariantCulture);
            var finishDate = DepositInWork.FinishDate.ToString("d/MM/yyyy", CultureInfo.InvariantCulture);
            Junction = $"{SelectedDepositOffer.Bank.Header} {SelectedDepositOffer.Title} {startDate} - {finishDate} {rate:0.#}%";
        }

        public void FillDepositRatesTable()
        {
            var vm = new RulesAndRatesViewModel();
            vm.Initialize("", DepositInWork.StartDate, 
                SelectedDepositOffer.Essentials.LastOrDefault(p => p.Key < DepositInWork.StartDate).Value);
            _windowManager.ShowDialog(vm);
        }
    }
}
