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

        public InvestmentAssetsViewModel(ILifetimeScope globalScope, KeeperDataModel dataModel, IWindowManager windowManager)
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


        public void DeleteSelected()
        {
            if (SelectedAsset != null)
                Assets.Remove(SelectedAsset);
        }

        public override void CanClose(Action<bool> callback)
        {
            _dataModel.InvestmentAssets = Assets.ToList();
            base.CanClose(callback);
        }
    }
}
