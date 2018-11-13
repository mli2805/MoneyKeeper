using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Keeper2018
{
    [Serializable]
    public class KeeperBin
    {
        public List<OfficialRates> OfficialRates { get; set; }
        public List<Account> AccountPlaneList { get; set; }
        public List<Transaction> Transactions { get; set; }
        public ObservableCollection<TagAssociation> TagAssociations { get; set; }
        public List<DepositOffer> DepositOffers { get; set; }


    }

    public class KeeperDb
    {
        public KeeperBin Bin;
        public Dictionary<int, AccountModel> AcMoDict;

        public List<OfficialRates> OfficialRates { get; set; }
        public ObservableCollection<AccountModel> AccountsTree { get; set; }


        public ObservableCollection<TransactionModel> TransactionModels { get; set; }
        public ObservableCollection<TagAssociationModel> AssociationModels { get; set; }
        public ObservableCollection<DepositOfferModel> OfferModels { get; set; }

    }
}