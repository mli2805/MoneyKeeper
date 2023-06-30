using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using KeeperDomain;
using KeeperDomain.Exchange;

namespace Keeper2018
{
    public class KeeperDataModel : PropertyChangedBase
    {
        public Dictionary<DateTime, OfficialRates> OfficialRates { get; set; }
        public Dictionary<DateTime, ExchangeRates> ExchangeRates { get; set; }
        public List<MinfinMetalRate> MetalRates { get; set; }
        public List<RefinancingRate> RefinancingRates { get; set; } = new List<RefinancingRate>();

        public List<TrustAccount> TrustAccounts { get; set; }
        public List<InvestmentAssetModel> InvestmentAssets { get; set; }
        public List<AssetRate> AssetRates { get; set; }
        public List<InvestTranModel> InvestTranModels { get; set; }

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

        public ObservableCollection<AccountItemModel> AccountItems { get; } =
            new ObservableCollection<AccountItemModel>();


        public List<DepositOfferModel> DepositOffers { get; set; }
        public List<CarModel> Cars { get; set; }
        public List<FuellingModel> FuellingVms { get; set; }

        public List<ButtonCollectionModel> ButtonCollections { get; set; }
    }
}