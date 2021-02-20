using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class BankOffersViewModel : Screen
    {
        private readonly IWindowManager _windowManager;
        private readonly KeeperDataModel _dataModel;
        private readonly OneBankOfferViewModel _oneBankOfferViewModel;
        public ObservableCollection<DepositOfferModel> Rows { get; set; }

        private DepositOfferModel _selectedDepositOffer;
        public DepositOfferModel SelectedDepositOffer
        {
            get => _selectedDepositOffer;
            set
            {
                if (Equals(value, _selectedDepositOffer)) return;
                _selectedDepositOffer = value;
                NotifyOfPropertyChange();
            }
        }

        public BankOffersViewModel(IWindowManager windowManager, KeeperDataModel dataModel,
            OneBankOfferViewModel oneBankOfferViewModel)
        {
            _windowManager = windowManager;
            _dataModel = dataModel;
            _oneBankOfferViewModel = oneBankOfferViewModel;
        }

        public void Initialize()
        {
            Rows = new ObservableCollection<DepositOfferModel>
                (_dataModel.DepositOffers);
            SelectedDepositOffer = Rows.Last();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Банковские депозиты";
        }

        public void AddOffer()
        {
            var offerModel = new DepositOfferModel
            {
                Id = Rows.Max(l => l.Id) + 1, 
                Bank = SelectedDepositOffer.Bank, 
                MainCurrency = CurrencyCode.BYN,
            };

            _oneBankOfferViewModel.Initialize(offerModel);
            _windowManager.ShowDialog(_oneBankOfferViewModel);
            if (_oneBankOfferViewModel.IsCancelled) return;

            Rows.Add(_oneBankOfferViewModel.ModelInWork);
            _dataModel.DepositOffers = Rows.ToList();
            SelectedDepositOffer = Rows.Last();
        }

        public void EditSelectedOffer()
        {
            var offerModel = SelectedDepositOffer.DeepCopy();
            _oneBankOfferViewModel.Initialize(offerModel);
            _windowManager.ShowDialog(_oneBankOfferViewModel);
            if (_oneBankOfferViewModel.IsCancelled) return;

            var index = Rows.IndexOf(SelectedDepositOffer);
            Rows.Remove(SelectedDepositOffer);
            Rows.Insert(index, offerModel);
            SelectedDepositOffer = Rows[index];
            _dataModel.DepositOffers = Rows.ToList();
        }

        public void RemoveSelectedOffer()
        {
            if (_dataModel.AcMoDict.Values.Any(a=>a.IsDeposit && a.Deposit.DepositOfferId == SelectedDepositOffer.Id))
            {
                var strs = new List<string> {"Существует как минимум один депозит открытый по этой оферте.", "", "Сначала удалите депозиты."};
                var vm = new MyMessageBoxViewModel(MessageType.Error, strs);
                _windowManager.ShowDialog(vm);
                return;
            }
            Rows.Remove(SelectedDepositOffer);
            _dataModel.DepositOffers = Rows.ToList();
        }

        public override void CanClose(Action<bool> callback)
        {
            _dataModel.DepositOffers = Rows.ToList();
            base.CanClose(callback);
        }
    }
}
