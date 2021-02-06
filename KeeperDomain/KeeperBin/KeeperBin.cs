using System;
using System.Collections.Generic;

namespace KeeperDomain
{
    [Serializable]
    public class KeeperBin
    {
        public List<CurrencyRates> Rates { get; set; }

        public List<Account> AccountPlaneList { get; set; }
        public List<Deposit> Deposits { get; set; }
        public List<PayCard> PayCards { get; set; }

        public List<Transaction> Transactions { get; set; }

        public List<TagAssociation> TagAssociations { get; set; }
        public List<Car> Cars { get; set; }
        public List<YearMileage> YearMileages { get; set; }
        public List<Fuelling> Fuellings { get; set; }

        public List<DepositOffer> DepositOffers { get; set; }
        public List<DepositConditions> DepositConditions { get; set; }
        public List<DepositCalculationRules> DepositCalculationRules { get; set; }
        public List<DepositRateLine> DepositRateLines { get; set; }
    }
}