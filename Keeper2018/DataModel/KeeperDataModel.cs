using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class KeeperDataModel : PropertyChangedBase
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

        public ObservableCollection<TagAssociationModel> TagAssociationModels { get; set; }
    }
}