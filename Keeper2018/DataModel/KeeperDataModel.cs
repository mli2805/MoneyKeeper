using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class KeeperDataModel : PropertyChangedBase
    {
        public Dictionary<DateTime, CurrencyRates> Rates { get; set; }
        public List<Account> AccountPlaneList { get; set; }
        public Dictionary<int, Transaction> Transactions { get; set; }
        public List<TagAssociation> TagAssociations { get; set; }
        public List<DepositOffer> DepositOffers { get; set; }
        public List<Car> Cars { get; set; }
        public List<Fuelling> Fuellings { get; set; }


        public Dictionary<int, AccountModel> AcMoDict;

        private ObservableCollection<AccountModel> _accountsTree;
        public ObservableCollection<AccountModel> AccountsTree
        {
            get => _accountsTree;
            set
            {
                if (Equals(value, _accountsTree)) return;
                _accountsTree = value;
                NotifyOfPropertyChange();
            }
        }

        public List<FuellingVm> FuellingVms { get; set; }
    }
}