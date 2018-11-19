using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class DepositOffersViewModel : Screen
    {
        private readonly IWindowManager _windowManager;
        private readonly KeeperDb _db;
        private readonly OneDepositOfferViewModel _oneDepositOfferViewModel;
        public ObservableCollection<DepositOfferModel> Rows { get;set; }

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

        public DepositOffersViewModel(IWindowManager windowManager, KeeperDb db, 
            OneDepositOfferViewModel oneDepositOfferViewModel)
        {
            _windowManager = windowManager;
            _db = db;
            _oneDepositOfferViewModel = oneDepositOfferViewModel;
        }

        public void Initialize()
        {
            Rows = new ObservableCollection<DepositOfferModel>
                (_db.Bin.DepositOffers.Select(x=>x.Map(_db.Bin.AccountPlaneList)));
            SelectedDepositOffer = Rows.Last();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Банковские депозиты";
        }

        public void AddOffer()
        {
            _oneDepositOfferViewModel.Initialize(null);
            _windowManager.ShowDialog(_oneDepositOfferViewModel);
        }

        public void EditSelectedOffer()
        {
            _oneDepositOfferViewModel.Initialize(SelectedDepositOffer);
            _windowManager.ShowDialog(_oneDepositOfferViewModel);
        }

        public void RemoveSelectedOffer()
        {
            Rows.Remove(SelectedDepositOffer);
        }

        public override void CanClose(Action<bool> callback)
        {
            _db.Bin.DepositOffers = new List<DepositOffer>(Rows.Select(d=>d.Map()));
            base.CanClose(callback);
        }
    }
}
