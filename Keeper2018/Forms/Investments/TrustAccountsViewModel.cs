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

        public ObservableCollection<TrustAccount> TrustAccounts { get; set; }
        public TrustAccount SelectedAccount { get; set; }


        public TrustAccountsViewModel(KeeperDataModel dataModel, IWindowManager windowManager, 
            TrustAccountStateViewModel trustAccountStateViewModel)
        {
            _dataModel = dataModel;
            _windowManager = windowManager;
            _trustAccountStateViewModel = trustAccountStateViewModel;
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

        public void DeleteSelected()
        {
            if (SelectedAccount != null)
                TrustAccounts.Remove(SelectedAccount);
        }

        public override void CanClose(Action<bool> callback)
        {
            _dataModel.TrustAccounts = TrustAccounts.ToList();
            base.CanClose(callback);
        }
    }
}
