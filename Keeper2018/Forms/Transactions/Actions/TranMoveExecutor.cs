using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public class TranMoveExecutor
    {
        public enum Destination { Up, Down }

        private readonly TransModel _model;

        public TranMoveExecutor(TransModel model)
        {
            _model = model;
        }

        private List<TransactionModel> _selectedTransactions;
        private List<TransactionModel> _transToElevate;
        private List<TransactionModel> _nearbyTransactions;
        private List<TransactionModel> _transToLower;
        private List<TransactionModel> _transToShiftTime;

        private bool FillLists(Destination destination)
        {
            var selected = _model.SelectedTranWrappedForDatagrid.Tran;
            if (selected.Receipt != 0)
            {
                _selectedTransactions = _model.Rows.Where(t =>
                    t.Tran.Timestamp.Date.Equals(selected.Timestamp.Date) && t.Tran.Receipt == selected.Receipt).Select(r => r.Tran).ToList();

                var edge = destination == Destination.Up ? _selectedTransactions.Min(t => t.Timestamp) : _selectedTransactions.Max(t => t.Timestamp);
                if (!selected.Timestamp.Equals(edge))
                    _selectedTransactions = new List<TransactionModel>{selected};
            }
            else _selectedTransactions = new List<TransactionModel> {selected};

            var nearbyTran = _model.Rows.LastOrDefault(t => t.Tran.Timestamp < selected.Timestamp);
            if (nearbyTran == null) return false;

            if (nearbyTran.Tran.Receipt != 0)
                _nearbyTransactions = _model.Rows.Where(t => t.Tran.Timestamp.Date.Equals(nearbyTran.Tran.Timestamp.Date)
                                                       && t.Tran.Receipt == nearbyTran.Tran.Receipt).Select(r => r.Tran).ToList();
            else _nearbyTransactions = new List<TransactionModel> {nearbyTran.Tran};

            _transToElevate = destination == Destination.Up ? _selectedTransactions : _nearbyTransactions;
            _transToLower = destination == Destination.Up ?  _nearbyTransactions : _selectedTransactions;

            if (destination == Destination.Down && selected.Timestamp.Date != nearbyTran.Tran.Timestamp.Date)
            {
                var maxOfElevate = _transToElevate.Max(t => t.Timestamp);
                _transToShiftTime = _model.Rows.Where(t => t.Tran.Timestamp.Date.Equals(maxOfElevate.Date) 
                                             && t.Tran.Timestamp.Date > maxOfElevate).Select(r => r.Tran).ToList();
            }
            else _transToShiftTime = new List<TransactionModel>();

            return true;
        }

        public void MoveSelected(Destination destination)
        {
            if (!FillLists(destination)) return;

            if (_transToElevate.First().Timestamp.Date.Equals(_transToLower.First().Timestamp.Date))
            {
                var newTimestamp = _transToLower.Min(t => t.Timestamp);
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

        private void SetNewTimes(DateTime newTimestamp)
        {
            foreach (var transactionModel in _selectedTransactions)
            {
                transactionModel.Timestamp = newTimestamp;
                newTimestamp = newTimestamp.AddMinutes(1);
            }

            foreach (var transactionModel in _nearbyTransactions)
            {
                transactionModel.Timestamp = newTimestamp;
                newTimestamp = newTimestamp.AddMinutes(1);
            }

            foreach (var transactionModel in _transToShiftTime)
            {
                transactionModel.Timestamp = newTimestamp;
                newTimestamp = newTimestamp.AddMinutes(1);
            }
        }

    }
}