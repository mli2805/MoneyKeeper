using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class OneAssetViewModel : Screen
    {
        private readonly KeeperDataModel _keeperDataModel;
        private string _aim;
        public List<PeriodUnit> PeriodUnits { get; set; }
        public List<StockMarket> StockMarkets { get; set; }
        public List<AssetType> AssetTypes { get; set; }

        public InvestmentAssetModel AssetInWork { get; set; }

        public OneAssetViewModel(KeeperDataModel keeperDataModel)
        {
            _keeperDataModel = keeperDataModel;
            PeriodUnits = Enum.GetValues(typeof(PeriodUnit)).OfType<PeriodUnit>().ToList();
            StockMarkets = Enum.GetValues(typeof(StockMarket)).OfType<StockMarket>().ToList();
            AssetTypes = Enum.GetValues(typeof(AssetType)).OfType<AssetType>().Take(2).ToList();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _aim;
        }

        public void Initialize(int id)
        {
            _aim = "Добавить бумагу";
            AssetInWork = new InvestmentAssetModel()
            {
                Id = id,
                StockMarket = StockMarket.Russia,
                AssetType = AssetType.Stock,

                PreviousCouponDate = DateTime.Today.AddDays(-1),
                BondCouponPeriod = new CalendarPeriod() { Value = 182, Unit = PeriodUnit.days },
                BondExpirationDate = DateTime.Today.AddYears(1),
            };
        }

        public void Initialize(InvestmentAssetModel assetModel)
        {
            _aim = "Изменить бумагу";
            AssetInWork = assetModel.Clone();
        }

        public void Save()
        {
            AssetInWork.TrustAccount = _keeperDataModel.TrustAccounts.First(t => t.StockMarket == AssetInWork.StockMarket);
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }
    }
}
