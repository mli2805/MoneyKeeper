using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;

namespace Keeper2018
{
    public class TranModel : PropertyChangedBase
    {
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
        public ICollectionView SortedRows { get; set; }

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

        public DateTime AskedDate { get; set; } = DateTime.Now;
        public bool IsCollectionChanged { get; set; }

        private readonly KeeperDataModel _dataModel;
        private readonly FilterModel _filterModel;
        private TranFilter _tranFilter;


        public TranModel(KeeperDataModel dataModel, FilterModel filterModel)
        {
            _dataModel = dataModel;
            _filterModel = filterModel;
        }

        public void Initialize()
        {
            _tranFilter = new TranFilter();

            // Rows = WrapTransactions(_dataModel.Transactions);
            Rows = new ObservableCollection<TranWrappedForDatagrid>(
                _dataModel.Transactions.Values.Select(t => new TranWrappedForDatagrid(t)));
            Rows.CollectionChanged += Rows_CollectionChanged;

            SortedRows = CollectionViewSource.GetDefaultView(Rows);
            SortedRows.Filter += Filter;
            SortedRows.SortDescriptions.Add(new SortDescription("Tran.Timestamp", ListSortDirection.Ascending));
            SortedRows.CurrentChanged += SortedRows_CurrentChanged;

            SortedRows.MoveCurrentToLast();
            SelectedTranWrappedForDatagrid = (TranWrappedForDatagrid)SortedRows.CurrentItem;
            SelectedTranWrappedForDatagrid.IsSelected = true;

            IsCollectionChanged = false;
        }

        private void SortedRows_CurrentChanged(object sender, EventArgs e)
        {
            var wrapped = (TranWrappedForDatagrid)((ICollectionView) sender).CurrentItem;
            if (wrapped != null)
                Console.WriteLine($@"current tran is {wrapped.Tran.Timestamp}");
        }

        private bool Filter(object o)
        {
            var t = (TranWrappedForDatagrid)o;
            return _tranFilter.Filter(t, _filterModel);
        }
        private void Rows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IsCollectionChanged = true;
        }

        // private ObservableCollection<TranWrappedForDatagrid> WrapTransactions(Dictionary<int, TransactionModel> transactions)
        // {
        //     var result = new ObservableCollection<TranWrappedForDatagrid>();
        //     foreach (var tran in transactions.Values)
        //     {
        //         var wrapped = new TranWrappedForDatagrid() { Tran = tran };
        //         result.Add(wrapped);
        //     }
        //     return result;
        // }
    }
}