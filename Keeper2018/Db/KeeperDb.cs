using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class KeeperDb : PropertyChangedBase
    {
        public KeeperBin Bin;
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

        public List<DepositOffer> DepoOffers; // temp! compare with DepositOffers in bin! if equal parse into old list at once!

        public ObservableCollection<TagAssociationModel> TagAssociationModels { get; set; }
    }
}