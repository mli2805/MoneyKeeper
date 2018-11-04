using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace Keeper2018
{
    public class TransactionsViewModel : Screen
    {
        private readonly KeeperDb _keeperDb;

        public ObservableCollection<Transaction> Rows { get; set; }

        public TransactionsViewModel(KeeperDb keeperDb)
        {
            _keeperDb = keeperDb;
        }

    }
}
