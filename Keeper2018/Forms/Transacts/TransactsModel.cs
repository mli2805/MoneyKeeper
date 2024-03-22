using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;

namespace Keeper2018
{
    public class TransactsModel : PropertyChangedBase
    {
        private readonly KeeperDataModel _dataModel;
        private readonly FilterModel _filterModel;
        private TranFilter _tranFilter;

        public ObservableCollection<TranWrappedForDataGrid> Rows { get; set; }

        private TranWrappedForDataGrid _selectedTranWrappedForDataGrid;
        public TranWrappedForDataGrid SelectedTranWrappedForDataGrid
        {
            get => _selectedTranWrappedForDataGrid;
            set
            {
                if (Equals(value, _selectedTranWrappedForDataGrid)) return;
                _selectedTranWrappedForDataGrid = value;
                NotifyOfPropertyChange(() => SelectedTranWrappedForDataGrid);
            }
        }

        public ICollectionView SortedRows { get; set; }

        public DateTime AskedDate { get; set; } = DateTime.Now;
        public bool IsCollectionChanged { get; set; }
      
        public TransactsModel(KeeperDataModel dataModel, FilterModel filterModel)
        {
            _dataModel = dataModel;
            _filterModel = filterModel;
        }

        public void Initialize()
        {
            _tranFilter = new TranFilter();
           
            Rows = new ObservableCollection<TranWrappedForDataGrid>(
                _dataModel.Transactions.Values.Select(t=>new TranWrappedForDataGrid(t)));
            SelectedTranWrappedForDataGrid = Rows.Last();
            Rows.CollectionChanged += Rows_CollectionChanged;

            SortedRows = CollectionViewSource.GetDefaultView(Rows);
            SortedRows.Filter += Filter;
            SortedRows.SortDescriptions.Add(new SortDescription("Tran.Timestamp", ListSortDirection.Ascending));

            SortedRows.MoveCurrentToLast();
            SelectedTranWrappedForDataGrid = (TranWrappedForDataGrid)SortedRows.CurrentItem;
            SelectedTranWrappedForDataGrid.IsSelected = true;
        }

        private bool Filter(object o)
        {
            var t = (TranWrappedForDataGrid)o;
            return _tranFilter.Filter(t, _filterModel);
        }

        private void Rows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IsCollectionChanged = true;
        }
    }
}