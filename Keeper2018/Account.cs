using System.Collections.ObjectModel;

namespace Keeper2018
{
    public class Account
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public Account Parent { get; set; }
        public ObservableCollection<Account> Children { get; set; }
        public int DepositCode { get; set; } = -1;
    }
}