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
        private ObservableCollection<AccountItemModel> _accountsTree = new ObservableCollection<AccountItemModel>();
        public Dictionary<DateTime, OfficialRates> OfficialRates { get; set; }
        public Dictionary<DateTime, ExchangeRates> ExchangeRates { get; set; }
        public List<MinfinMetalRate> MetalRates { get; set; }
        public List<RefinancingRate> RefinancingRates { get; set; } = new List<RefinancingRate>();

        public List<TrustAccount> TrustAccounts { get; set; }
        public List<InvestmentAssetModel> InvestmentAssets { get; set; }
        public List<AssetRate> AssetRates { get; set; }
        public List<InvestTranModel> InvestTranModels { get; set; }

        public Dictionary<int, TransactionModel> Transactions { get; set; }


        public ObservableCollection<AccountItemModel> AccountsTree
        {
            get => _accountsTree;
            set
            {
                if (Equals(value, _accountsTree)) return;
                _accountsTree = value;
                NotifyOfPropertyChange();
            }
        }

        public Dictionary<int, AccountItemModel> AcMoDict { get; set; } =
            new Dictionary<int, AccountItemModel>();

        public List<DepositOfferModel> DepositOffers { get; set; }
        public List<CarModel> Cars { get; set; }
        public List<FuellingModel> FuellingVms { get; set; }

        public List<ButtonCollectionModel> ButtonCollections { get; set; }
    }
}