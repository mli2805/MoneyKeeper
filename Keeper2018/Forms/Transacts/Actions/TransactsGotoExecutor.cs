﻿using System.Linq;

namespace Keeper2018
{
    public class TransactsGotoExecutor
    {
        private readonly TransactsModel _model;

        public TransactsGotoExecutor(TransactsModel model)
        {
            _model = model;
        }

        public void SelectFirstOfDate()
        {
            var askedTran = _model.Rows.FirstOrDefault(t => t.Tran.Timestamp >= _model.AskedDate);
            if (askedTran != null)
            {
                _model.SelectedTranWrappedForDataGrid.IsSelected = false;

                _model.SortedRows.MoveCurrentTo(askedTran);
                _model.SelectedTranWrappedForDataGrid = (TranWrappedForDataGrid)_model.SortedRows.CurrentItem;
            }
        }

        public void SelectLast()
        {
            _model.SelectedTranWrappedForDataGrid.IsSelected = false;

            _model.SortedRows.MoveCurrentToLast();
            _model.SelectedTranWrappedForDataGrid = (TranWrappedForDataGrid)_model.SortedRows.CurrentItem;
        }
    }
}