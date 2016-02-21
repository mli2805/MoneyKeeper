using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Composition;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.DomainModel.Transactions;
using Keeper.Utils;

namespace Keeper.ViewModels.Transactions
{
    [Export]
    class TrViewModel : Screen
    {
        private readonly KeeperDb _db;
        private ObservableCollection<TranCocoon> _rows;
        private TranCocoon _selectedTranCocoon;

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

        public TranCocoon SelectedTranCocoon
        {
            get { return _selectedTranCocoon; }
            set
            {
                if (Equals(value, _selectedTranCocoon)) return;
                _selectedTranCocoon = value;
                NotifyOfPropertyChange();
            }
        }

        public TranActions ActionsHandler { get; set; }

        [ImportingConstructor]
        public TrViewModel(KeeperDb db)
        {
            _db = db;
            new TransactionsConvertor(_db).Convert();
            Rows = WrapTransactions(_db.TransWithTags);
            var sortedRows = CollectionViewSource.GetDefaultView(Rows);
            sortedRows.SortDescriptions.Add(new SortDescription("Tran.Timestamp", ListSortDirection.Ascending));

            Rows.Last().IsSelected = true;
            SelectedTranCocoon = Rows.Last();

            ActionsHandler = new TranActions(Rows, SelectedTranCocoon);
        }

        private ObservableCollection<TranCocoon> WrapTransactions(ObservableCollection<TranWithTags> transactions)
        {
            var result = new ObservableCollection<TranCocoon>();
            foreach (var tran in transactions)
            {
                result.Add(new TranCocoon() { Tran = tran });
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

        public void ActionsMethod(int code)
        {
            ActionsHandler.Do(code);
        }
    }
}
