using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace Keeper2018
{
    public class TransactionsViewModel : Screen
    {
        private readonly KeeperDb _keeperDb;

        public ObservableCollection<TranWrappedForDatagrid> Rows { get; set; }

        private TranWrappedForDatagrid _selectedRow;
        public TranWrappedForDatagrid SelectedRow
        {
            get { return _selectedRow; }
            set
            {
                if (Equals(value, _selectedRow)) return;
                _selectedRow = value;
                NotifyOfPropertyChange();
            }
        }

        public TransactionsViewModel(KeeperDb keeperDb)
        {
            _keeperDb = keeperDb;
        }

        public void Init()
        {
            Rows = new ObservableCollection<TranWrappedForDatagrid>();
            foreach (var tran in _keeperDb.TransactionModels)
            {
                Rows.Add(new TranWrappedForDatagrid() { Tran = tran });
            }
        }

    }
}
