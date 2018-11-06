using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Keeper2018
{
    [Serializable]
    public class KeeperDb
    {
        public List<Account> AccountPlaneList { get; set; }

        [NonSerialized]
        private ObservableCollection<AccountModel> _accountsTree;
        public ObservableCollection<AccountModel> AccountsTree 
        {
            get => _accountsTree;
            set => _accountsTree = value;
        }

        public List<OfficialRates> OfficialRates { get; set; }
        public List<Transaction> Transactions { get; set; }
        public List<TagAssociation> TagAssociations { get; set; }
        public List<DepositOffer> DepositOffers { get;set; }
    }
}