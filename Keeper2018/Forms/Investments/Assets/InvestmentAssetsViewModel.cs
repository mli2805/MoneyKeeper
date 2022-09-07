using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class InvestmentAssetsViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly KeeperDataModel _dataModel;
        private readonly IWindowManager _windowManager;

        public ObservableCollection<InvestmentAssetModel> Assets { get; set; }
        public InvestmentAssetModel SelectedAsset { get; set; }

        public List<AssetType> AssetTypes { get; set; }

        public InvestmentAssetsViewModel(ILifetimeScope globalScope, KeeperDataModel dataModel,
            IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _dataModel = dataModel;
            _windowManager = windowManager;
            AssetTypes = Enum.GetValues(typeof(AssetType)).OfType<AssetType>().ToList();
        }

        public void Initialize()
        {
            Assets = new ObservableCollection<InvestmentAssetModel>(_dataModel.InvestmentAssets.OrderBy(l => l.Id));
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Инвестиционные активы";
        }

        public void ShowAssetAnalysis()
        {
            var vm = _globalScope.Resolve<AssetAnalysisViewModel>();
            vm.Initialize(SelectedAsset);
            _windowManager.ShowWindow(vm);
        }

        public void ShowAssetStatistics()
        {
            var vm = _globalScope.Resolve<AssetStatisticsViewModel>();
            vm.Initialize(SelectedAsset);
            _windowManager.ShowWindow(vm);
        }

        public void AddNewAsset()
        {
            var vm = _globalScope.Resolve<OneAssetViewModel>();
            vm.Initialize(Assets.Last().Id + 1);
            if (_windowManager.ShowDialog(vm) == true)
                Assets.Add(vm.AssetInWork);
        }

        public void EditAsset()
        {
            var vm = _globalScope.Resolve<OneAssetViewModel>();
            vm.Initialize(SelectedAsset);
            if (_windowManager.ShowDialog(vm) == true)
                SelectedAsset.CopyFrom(vm.AssetInWork);
        }

        public void DeleteSelected()
        {
            if (SelectedAsset != null)
                Assets.Remove(SelectedAsset);
        }

        public override void CanClose(Action<bool> callback)
        {
            foreach (var investmentAssetModel in Assets)
            {
                if (investmentAssetModel.TrustAccount == null)
                    investmentAssetModel.TrustAccount =
                        _dataModel.TrustAccounts.First(ta => ta.StockMarket == investmentAssetModel.StockMarket);
            }
            _dataModel.InvestmentAssets = Assets.ToList();
            base.CanClose(callback);
        }
    }
}
