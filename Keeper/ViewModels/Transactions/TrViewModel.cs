using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Composition;
using System.Windows.Data;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.DomainModel.Transactions;
using Keeper.Utils;

namespace Keeper.ViewModels.Transactions
{
    class TrViewModel : Screen
    {
        private readonly KeeperDb _db;
        private ObservableCollection<TranCocoon> _rows;
        public ObservableCollection<TranCocoon> Rows
        {
            get { return _rows; }
            set
            {
                if (Equals(value, _rows)) return;
                _rows = value;
                NotifyOfPropertyChange();
            }
        }

//        public string EndDayBalances { get {return new AccountBalanceOnTrCalculator(_db).EndDayBalances(DateTime.Now); } }

        [ImportingConstructor]
        public TrViewModel(KeeperDb db)
        {
            _db = db;
            new TransactionsConvertor(_db).Convert();
            Rows = WrapTransactions(_db.TransWithTags);
            var sortedRows = CollectionViewSource.GetDefaultView(Rows);
            sortedRows.SortDescriptions.Add(new SortDescription("Tran.Timestamp", ListSortDirection.Ascending));

        }

        private ObservableCollection<TranCocoon> WrapTransactions(ObservableCollection<TranWithTags> transactions)
        {
            var result = new ObservableCollection<TranCocoon>();
            foreach (var tran in transactions)
            {
                result.Add(new TranCocoon() {Tran = tran});
            }
            return result;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Transactions with tags";
        }

        public void Close()
        {
            TryClose();
        }
    }
}
