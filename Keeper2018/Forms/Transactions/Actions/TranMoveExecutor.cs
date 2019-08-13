using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public class TranMoveExecutor
    {
        public enum Destination { Up, Down }

        private readonly TranModel _model;
        private readonly KeeperDb _db;

        public TranMoveExecutor(TranModel model, KeeperDb db)
        {
            _model = model;
            _db = db;
        }

        private List<TransactionModel> _selectedTransactions;
        private List<TransactionModel> _nearbyTransactions;
        private List<TransactionModel> _transToElevate;
        private List<TransactionModel> _transToLower;
        private List<TransactionModel> _transToShiftTime;
        private bool _areDatesEqual;
        public void MoveSelected(Destination destination)
        {
             if (!FillLists(destination)) return;

            if (!_areDatesEqual && destination == Destination.Down)
            {
                var newTimestamp = _transToElevate.Min(t => t.Timestamp);
                SetNewTimes(newTimestamp);
            }
            else
            {
                var newTimestamp = _transToLower.Min(t => t.Timestamp);
               SetNewTimes(newTimestamp);
            }
           
            _model.SortedRows.Refresh();
            _model.IsCollectionChanged = true;
        }

        private bool FillLists(Destination destination)
        {
            GetSelected(destination);
            if (!GetNearby(destination)) return false;

            _transToElevate = destination == Destination.Up ? _selectedTransactions : _nearbyTransactions;
            _transToLower = destination == Destination.Up ?  _nearbyTransactions : _selectedTransactions;

            if (!_areDatesEqual && _selectedTransactions.First().Receipt != 0)
            {
                var maxReceipt = _model.Rows
                    .Where(t => t.Tran.Timestamp.Date.Equals(_nearbyTransactions.First().Timestamp.Date))
                    .Max(l => l.Tran.Receipt);
                foreach (var tran in _selectedTransactions)
                {
                    tran.Receipt = maxReceipt + 1;
                }
            }

            if (destination == Destination.Down && !_areDatesEqual)
            {
                var maxOfElevate = _transToElevate.Max(t => t.Timestamp);
                _transToShiftTime = _model.Rows.Where(t => t.Tran.Timestamp.Date.Equals(maxOfElevate.Date) 
                                                           && t.Tran.Timestamp > maxOfElevate).Select(r => r.Tran).ToList();
            }
            else _transToShiftTime = new List<TransactionModel>();

            return true;
        }

        private void GetSelected(Destination destination)
        {
            var selected = _model.SelectedTranWrappedForDatagrid.Tran;
            if (selected.Receipt != 0)
            {
                _selectedTransactions = _model.Rows.Where(t =>
                        t.Tran.Timestamp.Date.Equals(selected.Timestamp.Date) && t.Tran.Receipt == selected.Receipt)
                    .Select(r => r.Tran).OrderBy(f=>f.Timestamp).ToList();

                var edgeOfReceipt = destination == Destination.Up
                    ? _selectedTransactions.Min(t => t.Timestamp)
                    : _selectedTransactions.Max(t => t.Timestamp);
                if (!selected.Timestamp.Equals(edgeOfReceipt))
                    _selectedTransactions = new List<TransactionModel> {selected};
            }
            else _selectedTransactions = new List<TransactionModel> {selected};
        }

        private bool GetNearby(Destination destination)
        {
            var nearbyTran = destination == Destination.Up
                ? _model.Rows.OrderBy(r => r.Tran.Timestamp)
                    .LastOrDefault(t => t.Tran.Timestamp < _selectedTransactions.First().Timestamp)
                : _model.Rows.OrderBy(r => r.Tran.Timestamp)
                    .FirstOrDefault(t => t.Tran.Timestamp > _selectedTransactions.Last().Timestamp);
            if (nearbyTran == null) return false;

            _areDatesEqual = _selectedTransactions.First().Timestamp.Date.Equals(nearbyTran.Tran.Timestamp.Date);
            if (nearbyTran.Tran.Receipt != 0)
            {
                var isReceiptEqual = _areDatesEqual && _selectedTransactions.First().Receipt == nearbyTran.Tran.Receipt;
                _nearbyTransactions = isReceiptEqual
                    ? new List<TransactionModel> {nearbyTran.Tran}
                    : _model.Rows.Where(t => t.Tran.Timestamp.Date.Equals(nearbyTran.Tran.Timestamp.Date)
                           && t.Tran.Receipt == nearbyTran.Tran.Receipt).Select(r => r.Tran).OrderBy(f=>f.Timestamp).ToList();
            }
            else _nearbyTransactions = new List<TransactionModel> {nearbyTran.Tran};
            return true;
        }

        private void SetNewTimes(DateTime newTimestamp)
        {
           newTimestamp = SetNewTimes(newTimestamp, _transToElevate);
           newTimestamp = SetNewTimes(newTimestamp, _transToLower);
           SetNewTimes(newTimestamp, _transToShiftTime);
        }

        private DateTime SetNewTimes(DateTime newTimestamp, List<TransactionModel> list)
        {
            foreach (var transactionModel in list)
            {
                transactionModel.Timestamp = newTimestamp;
                var transaction = _db.Bin.Transactions[transactionModel.TransactionKey];
                transaction.Timestamp = newTimestamp;
                newTimestamp = newTimestamp.AddMinutes(1);
            }
            return newTimestamp;
        }

    }
}