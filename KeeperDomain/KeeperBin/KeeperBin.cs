using System;
using System.Collections.Generic;
using KeeperDomain.Exchange;

namespace KeeperDomain
{
    interface IDumpable
    {
        string Dump();
    }

    interface IParsable<T>
    {
        T FromString(string s);
    }

    [Serializable]
    public class KeeperBin
    {
        public List<OfficialRates> OfficialRates { get; set; }
        public List<ExchangeRates> ExchangeRates { get; set; }
        public List<MetalRate> MetalRates { get; set; }
        public List<RefinancingRate> RefinancingRates { get; set; }

        public List<TrustAccount> TrustAccounts { get; set; }
        public List<TrustAsset> TrustAssets { get; set; }
        public List<TrustAssetRate> TrustAssetRates { get; set; }
        public List<TrustTransaction> TrustTransactions { get; set; }

        public List<Account> AccountPlaneList { get; set; }
        public List<BankAccount> BankAccounts { get; set; }
        public List<Deposit> Deposits { get; set; }
        public List<PayCard> PayCards { get; set; }
        public List<ButtonCollection> ButtonCollections { get; set; }

        public List<Transaction> Transactions { get; set; }

        public List<Car> Cars { get; set; }
        public List<CarYearMileage> CarYearMileages { get; set; }
        public List<Fuelling> Fuellings { get; set; }

        public List<DepositOffer> DepositOffers { get; set; }
        public List<DepositRateLine> DepositRateLines { get; set; }
        public List<DepositConditions> DepositConditions { get; set; }

        public List<CardBalanceMemo> CardBalanceMemos { get; set; }
        public List<SalaryChange> SalaryChanges { get; set; }
        public List<LargeExpenseThreshold> LargeExpenseThresholds { get; set; }

    }
}