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

        public ObservableCollection<TrustAccount> TrustAccounts { get; set; }
        public TrustAccount SelectedAccount { get; set; }

        public TrustAccountsViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Initialize()
        {
            TrustAccounts = new ObservableCollection<TrustAccount>(_dataModel.TrustAccounts);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Трастовые счета";
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
