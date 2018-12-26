﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Caliburn.Micro;

namespace Keeper2018
{
    public class TransModel : PropertyChangedBase
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
        public int SelectedTranIndex { get; set; }
        public DateTime AskedDate { get; set; } = DateTime.Now;
        public bool IsCollectionChanged { get; set; }

        private readonly KeeperDb _db;
        private FilterModel _filterModel;


        private TranFilter _tranFilter;
        public TransModel(KeeperDb db, FilterModel filterModel)
        {
            _db = db;
            _filterModel = filterModel;
        }

        public void Initialize()
        {
            _tranFilter = new TranFilter();

            Rows = WrapTransactions(_db.TransactionModels);
            Rows.CollectionChanged += Rows_CollectionChanged;

            SortedRows = CollectionViewSource.GetDefaultView(Rows);
            SortedRows.Filter += Filter;
            SortedRows.SortDescriptions.Add(new SortDescription("Tran.Timestamp", ListSortDirection.Ascending));

            SortedRows.MoveCurrentToLast();
            SelectedTranWrappedForDatagrid = (TranWrappedForDatagrid)SortedRows.CurrentItem;
            SelectedTranWrappedForDatagrid.IsSelected = true;

            IsCollectionChanged = false;

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
        private ObservableCollection<TranWrappedForDatagrid> WrapTransactions(ObservableCollection<TransactionModel> transactions)
        {
            var result = new ObservableCollection<TranWrappedForDatagrid>();
            foreach (var tran in transactions)
            {
                result.Add(new TranWrappedForDatagrid() { Tran = tran });
            }
            return result;
        }
    }
}