using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Composition;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Transactions;
using Keeper.Utils;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    class TransViewModel : Screen
    {
        private readonly KeeperDb _db;
        private ObservableCollection<TranWrappedForDatagrid> _rows;
        public ObservableCollection<TranWrappedForDatagrid> Rows
        {
            get { return _rows; }
            set
            {
                if (Equals(value, _rows)) return;
                _rows = value;
                NotifyOfPropertyChange();
            }
        }

        private TranWrappedForDatagrid _selectedTranWrappedForDatagrid;
        public TranWrappedForDatagrid SelectedTranWrappedForDatagrid
        {
            get { return _selectedTranWrappedForDatagrid; }
            set
            {
                if (Equals(value, _selectedTranWrappedForDatagrid)) return;
                _selectedTranWrappedForDatagrid = value;
                NotifyOfPropertyChange();
            }
        }

        public TranActions ActionsHandler { get; set; }

        [ImportingConstructor]
        public TransViewModel(KeeperDb db)
        {
            _db = db;
            IoC.Get<TransactionsConvertor>().Convert();
            Rows = WrapTransactions(_db.TransWithTags);
            var sortedRows = CollectionViewSource.GetDefaultView(Rows);
            sortedRows.SortDescriptions.Add(new SortDescription("Tran.Timestamp", ListSortDirection.Ascending));

            Rows.Last().IsSelected = true;
            SelectedTranWrappedForDatagrid = Rows.Last();

            ActionsHandler = new TranActions();
        }

        private ObservableCollection<TranWrappedForDatagrid> WrapTransactions(ObservableCollection<TranWithTags> transactions)
        {
            var result = new ObservableCollection<TranWrappedForDatagrid>();
            foreach (var tran in transactions)
            {
                result.Add(new TranWrappedForDatagrid() { Tran = tran });
            }
            return result;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Transactions with tags";
        }

        public void ButtonClose()
        {
            TryClose();
        }

        public void ActionsMethod(int code)
        {
            var selectedItem = SelectedTranWrappedForDatagrid;
            ActionsHandler.Do(code, Rows, ref selectedItem);
            SelectedTranWrappedForDatagrid = selectedItem;
        }
    }
}
