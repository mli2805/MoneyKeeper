using System;
using System.Collections.Generic;

namespace Keeper2018
{
    [Serializable]
    public class KeeperBin
    {
        public List<OfficialRates> OfficialRates { get; set; }
        public List<Account> AccountPlaneList { get; set; }
        public List<Transaction> Transactions { get; set; }
        public List<TagAssociation> TagAssociations { get; set; }
        public List<DepositOffer> DepositOffers { get; set; }
    }
}