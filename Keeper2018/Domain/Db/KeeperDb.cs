using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;

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


        public ObservableCollection<TransactionModel> TransactionModels { get; set; }
        public ObservableCollection<LineModel> AssociationModels { get; set; }
    }
}