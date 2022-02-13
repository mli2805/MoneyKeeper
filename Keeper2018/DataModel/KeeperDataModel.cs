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
        public List<MinfinMetalRate> MetalRates { get; set; }

        public List<InvestmentAsset> InvestmentAssets { get; set; }
        public List<AssetRate> AssetRates { get; set; }
        public List<TrustAccount> TrustAccounts { get; set; }
        public List<InvestmentTransaction> InvestmentTransactions { get; set; }

        public Dictionary<int, TransactionModel> Transactions { get; set; }


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

        public List<DepositOfferModel> DepositOffers { get; set; }
        public List<CarModel> Cars { get; set; }
        public List<FuellingModel> FuellingVms { get; set; }
    }
}