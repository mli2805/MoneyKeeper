using System.Collections.ObjectModel;

namespace Keeper2018
{
    public class KeeperDb
    {
        public ObservableCollection<Account> Accounts { get; set; } = new ObservableCollection<Account>();
        public ObservableCollection<OfficialRates> OfficialRates { get; set; } = new ObservableCollection<OfficialRates>();
    }
}