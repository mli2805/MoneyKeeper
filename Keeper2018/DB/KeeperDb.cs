using System;
using System.Collections.ObjectModel;

namespace Keeper2018
{
    public class KeeperDb
    {
        public ObservableCollection<Account> Accounts { get; set; }
        public ObservableCollection<OfficialRates> OfficialRates { get; set; }
    }
}