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
            get { return _selectedDepositOffer; }
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
                (_dataModel.Bin.DepositOffers.Select(x => x.Map(_dataModel.Bin.AccountPlaneList)));
            SelectedDepositOffer = Rows.Last();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Банковские депозиты";
        }

        public void AddOffer()
        {
            var offer = new DepositOfferModel (Rows.Max(l => l.Id) + 1);

            _oneBankOfferViewModel.Initialize(offer);
            _windowManager.ShowDialog(_oneBankOfferViewModel);
            if (_oneBankOfferViewModel.IsCancelled) return;

            Rows.Add(_oneBankOfferViewModel.ModelInWork);
            SelectedDepositOffer = Rows.Last();
        }

        public void EditSelectedOffer()
        {
            var model = SelectedDepositOffer.DeepCopy();
            _oneBankOfferViewModel.Initialize(model);
            _windowManager.ShowDialog(_oneBankOfferViewModel);

            if (!_oneBankOfferViewModel.IsCancelled)
            {
                var index = Rows.IndexOf(SelectedDepositOffer);
                Rows.Remove(SelectedDepositOffer);
                SelectedDepositOffer = model.DeepCopy();
                Rows.Insert(index, SelectedDepositOffer);
            }
        }

        public void RemoveSelectedOffer()
        {
            if (_dataModel.Bin.AccountPlaneList.Any(a => a.IsDeposit && a.Deposit.DepositOfferId == SelectedDepositOffer.Id))
            {
                var strs = new List<string> {"Существует как минимум один депозит открытый по этой оферте.", "", "Сначала удалите депозиты."};
                var vm = new MyMessageBoxViewModel(MessageType.Error, strs);
                _windowManager.ShowDialog(vm);
                return;
            }
            Rows.Remove(SelectedDepositOffer);
        }

        public override void CanClose(Action<bool> callback)
        {
            _dataModel.Bin.DepositOffers = new List<DepositOffer>(Rows.Select(d => d.Map()));
            base.CanClose(callback);
        }
    }
}
