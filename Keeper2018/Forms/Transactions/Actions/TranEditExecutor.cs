﻿using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class TranEditExecutor
    {
        private readonly TransModel _model;

        private readonly IWindowManager _windowManager;
        private readonly KeeperDb _db;
        private readonly OneTranViewModel _oneTranViewModel;
        private readonly AskReceiptDeletionViewModel _askReceiptDeletionViewModel;

        public TranEditExecutor(TransModel model, IWindowManager windowManager,
            KeeperDb db, OneTranViewModel oneTranViewModel, AskReceiptDeletionViewModel askReceiptDeletionViewModel)
        {
            _model = model;

            _windowManager = windowManager;
            _db = db;
            _oneTranViewModel = oneTranViewModel;
            _askReceiptDeletionViewModel = askReceiptDeletionViewModel;
        }


        public void EditSelected()
        {
            var selectedTran = _model.SelectedTranWrappedForDatagrid.Tran;

            _oneTranViewModel.Init(selectedTran, false);
            bool? result = _windowManager.ShowDialog(_oneTranViewModel);

            if (!result.HasValue || !result.Value) return;

            _oneTranViewModel.GetTran().CopyInto(selectedTran);

            _db.Bin.Transactions.Remove(selectedTran.TransactionKey);
            _db.Bin.Transactions.Add(selectedTran.TransactionKey, selectedTran.Map());

            _model.SortedRows.Refresh();
            _model.IsCollectionChanged = true;
        }

        public void AddAfterSelected()
        {
            var tranForAdding = PrepareTranForAdding();
            _oneTranViewModel.Init(tranForAdding, true);
            bool? result = _windowManager.ShowDialog(_oneTranViewModel);

            if (!result.HasValue || !result.Value) return;

            if (_oneTranViewModel.ReceiptList != null)
                AddOneTranAndReceipt(_oneTranViewModel);
            else
                AddOneTran(_oneTranViewModel.GetTran().Clone());

            if (_oneTranViewModel.IsOneMore) AddAfterSelected();

            _model.IsCollectionChanged = true;
        }

        private void AddOneTran(TransactionModel tran)
        {
            tran.TransactionKey = _db.Bin.Transactions.Keys.Max() + 1;

            var wrappedTransactionsAfterInserted =
                _model.Rows.Where(t => t.Tran.Timestamp.Date == tran.Timestamp.Date && t.Tran.Timestamp >= tran.Timestamp).ToList();
            foreach (var wrapped in wrappedTransactionsAfterInserted)
            {
                wrapped.Tran.Timestamp = wrapped.Tran.Timestamp.AddMinutes(1);
                _db.Bin.Transactions[wrapped.Tran.TransactionKey].Timestamp = wrapped.Tran.Timestamp;
            }

            var tranWrappedForDatagrid = new TranWrappedForDatagrid() { Tran = tran };
            _model.Rows.Add(tranWrappedForDatagrid);
            _model.SelectedTranWrappedForDatagrid = tranWrappedForDatagrid;

            _db.Bin.Transactions.Add(tran.TransactionKey, tran.Map());
        }

        private void AddOneTranAndReceipt(OneTranViewModel oneTranForm)
        {
            var oneTran = oneTranForm.GetTran();
            var receiptId = _db.Bin.Transactions.Values.Where(t => t.Timestamp.Date == oneTran.Timestamp.Date)
                .Max(r => r.Receipt) + 1;
            foreach (var tuple in oneTranForm.ReceiptList)
            {
                var tran = oneTran.Clone();
                tran.Receipt = receiptId;
                tran.Amount = tuple.Item1;
                tran.Tags.Add(tuple.Item2);
                tran.Comment = tuple.Item3;
                AddOneTran(tran);
            }
        }
        private TransactionModel PrepareTranForAdding()
        {
            var tranForAdding = _model.SelectedTranWrappedForDatagrid.Tran.Clone();
            tranForAdding.Timestamp = tranForAdding.Timestamp.AddMinutes(1);
            tranForAdding.Amount = 0;
            tranForAdding.AmountInReturn = 0;
            tranForAdding.Comment = "";
            return tranForAdding;
        }

        public void DeleteSelected()
        {
            if (_model.SelectedTranWrappedForDatagrid.Tran.Receipt != 0)
            {
                _windowManager.ShowDialog(_askReceiptDeletionViewModel);
                if (_askReceiptDeletionViewModel.Result == 0)
                    return;
                if (_askReceiptDeletionViewModel.Result == 1)
                    DeleteOneTransaction();
                else
                    DeleteWholeReceipt();
            }
            else
                DeleteOneTransaction();
            _model.IsCollectionChanged = true;
        }

        private void DeleteOneTransaction()
        {
            var wrappedTrans = new List<TranWrappedForDatagrid>() { _model.SelectedTranWrappedForDatagrid };
            Delete(wrappedTrans);

        }
        private void DeleteWholeReceipt()
        {
            var wrappedTrans = _model.Rows.Where(t =>
                t.Tran.Timestamp.Date == _model.SelectedTranWrappedForDatagrid.Tran.Timestamp.Date
                && t.Tran.Receipt == _model.SelectedTranWrappedForDatagrid.Tran.Receipt).ToList();

            Delete(wrappedTrans);
        }

        private void Delete(List<TranWrappedForDatagrid> wrappedTrans)
        {
            int n = _model.Rows.IndexOf(wrappedTrans.First());
            foreach (var wrappedTran in wrappedTrans)
            {
                _db.Bin.Transactions.Remove(wrappedTran.Tran.TransactionKey);
                _model.Rows.Remove(wrappedTran);
            }

            if (n >= _model.Rows.Count)
                n = _model.Rows.Count - 1;
            _model.SelectedTranWrappedForDatagrid = _model.Rows.ElementAt(n);
        }
    }
}