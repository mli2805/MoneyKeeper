using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class TrustAccountsViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private readonly IWindowManager _windowManager;
        private readonly TrustAccountStateViewModel _trustAccountStateViewModel;
        private readonly TrustAccountAnalysisViewModel _trustAccountAnalysisViewModel;

        public ObservableCollection<TrustAccount> TrustAccounts { get; set; }
        public TrustAccount SelectedAccount { get; set; }


        public TrustAccountsViewModel(KeeperDataModel dataModel, IWindowManager windowManager, 
            TrustAccountStateViewModel trustAccountStateViewModel, TrustAccountAnalysisViewModel trustAccountAnalysisViewModel)
        {
            _dataModel = dataModel;
            _windowManager = windowManager;
            _trustAccountStateViewModel = trustAccountStateViewModel;
            _trustAccountAnalysisViewModel = trustAccountAnalysisViewModel;
        }

        public void Initialize()
        {
            TrustAccounts = new ObservableCollection<TrustAccount>(_dataModel.TrustAccounts);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Трастовые счета";
        }

        public void ShowTrustAccountState()
        {
            _trustAccountStateViewModel.Evaluate(SelectedAccount);
            _windowManager.ShowDialog(_trustAccountStateViewModel);
        }

        public void ShowTrustAccountAnalysis()
        {
            _trustAccountAnalysisViewModel.Initialize(SelectedAccount, DateTime.Today);
            _windowManager.ShowDialog(_trustAccountAnalysisViewModel);
        }

        public void DeleteSelected()
        {
            if (SelectedAccount != null)
                TrustAccounts.Remove(SelectedAccount);
        }

        public void Close()
        {
            TryClose();
        }

        public override void CanClose(Action<bool> callback)
        {
            _dataModel.TrustAccounts = TrustAccounts.ToList();
            base.CanClose(callback);
        }
    }
}
