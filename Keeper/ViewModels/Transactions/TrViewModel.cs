﻿using System;
using System.Collections.ObjectModel;
using System.Composition;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.DomainModel.Transactions;
using Keeper.Utils;

namespace Keeper.ViewModels.Transactions
{
    class TrViewModel : Screen
    {
        private readonly KeeperDb _db;
        private ObservableCollection<TrBase> _rows;
        public ObservableCollection<TrBase> Rows
        {
            get { return _rows; }
            set
            {
                if (Equals(value, _rows)) return;
                _rows = value;
                NotifyOfPropertyChange();
            }
        }

  //      public string EndDayBalances { get {  _balancesForTransactionsCalculator.EndDayBalances(DateTime.Today); } }

        [ImportingConstructor]
        public TrViewModel(KeeperDb db)
        {
            _db = db;
            new TransactionsConvertor(_db).Convert();
            Rows = _db.Trs;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Transactions with inheritance";
        }

        public void Close()
        {
            TryClose();
        }
    }
}
