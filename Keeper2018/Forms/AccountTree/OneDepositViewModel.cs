using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class OneDepositViewModel : Screen
    {
        private bool _isInAddMode;
        public Deposit DepositInWork { get; set; }
        private string _windowTitle;
        private readonly KeeperDb _db;
        private readonly IWindowManager _windowManager;

        private List<DepositOffer> _depositOffers;

        public List<DepositOffer> DepositOffers
        {
            get { return _depositOffers; }
            set
            {
                if (Equals(value, _depositOffers)) return;
                _depositOffers = value;
                NotifyOfPropertyChange();
            }
        }

        private DepositOffer _selectedDepositOffer;
        public DepositOffer SelectedDepositOffer
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

        public void InitializeForm(Deposit deposit, bool isInAddMode)
        {
            _isInAddMode = isInAddMode;
            DepositOffers = _db.Bin.DepositOffers;
            DepositInWork = deposit;

            if (isInAddMode)
            {
                SelectedDepositOffer = DepositOffers.First();
                DepositInWork.StartDate = DateTime.Today;
                DepositInWork.FinishDate = DateTime.Today.AddMonths(1);
            }
            else
            {
                _selectedDepositOffer = DepositOffers.First(o => o.Id == DepositInWork.DepositOfferId);
            }

        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _isInAddMode ? "Добавить" : "Изменить";
        }

        public void SaveDeposit()
        {
            TryClose(true);
        }

        public void CompileAccountName()
        {
//            var rate = DepositInWork.DepositOffer.RateLines == null || DepositInWork.DepositOffer.RateLines.LastOrDefault() == null
//                ? 0
//                : DepositInWork.DepositOffer.RateLines.Last().Rate;
//            Junction =
//                $"{DepositInWork.DepositOffer.BankAccount.Name} {DepositInWork.DepositOffer.DepositTitle} {DepositInWork.StartDate.ToString("d/MM/yyyy", CultureInfo.InvariantCulture)} - {DepositInWork.FinishDate.ToString("d/MM/yyyy", CultureInfo.InvariantCulture)} {rate:0.#}%";
        }

        public void FillDepositRatesTable()
        {
//            var bankDepositRatesAndRulesViewModel = IoC.Get<BankDepositRatesAndRulesViewModel>();
//            bankDepositRatesAndRulesViewModel.Initialize(DepositInWork.DepositOffer);
//            _windowManager.ShowDialog(bankDepositRatesAndRulesViewModel);
        }
    }
}
