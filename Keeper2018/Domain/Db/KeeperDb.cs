using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Keeper2018
{
    public class KeeperDb
    {
        public KeeperBin Bin;
        public Dictionary<int, AccountModel> AcMoDict;

        public List<OfficialRates> OfficialRates { get; set; }
        public ObservableCollection<AccountModel> AccountsTree { get; set; }


        public ObservableCollection<TransactionModel> TransactionModels { get; set; }
        public ObservableCollection<LineModel> AssociationModels { get; set; }
        public ObservableCollection<DepositOfferModel> OfferModels { get; set; }

    }
}