using System.Linq;

namespace Keeper2018
{
    public class TranSelectExecutor
    {
        private readonly TransModel _model;

        public TranSelectExecutor(TransModel model)
        {
            _model = model;
        }

        public void SelectFirstOfDate()
        {
            var askedTran = _model.Rows.FirstOrDefault(t => t.Tran.Timestamp >= _model.AskedDate);
            if (askedTran != null) _model.SelectedTranWrappedForDatagrid = askedTran;
        }

        public void SelectLast()
        {
//            _model.SelectedTranWrappedForDatagrid = _model.Rows.Last();
            _model.SortedRows.MoveCurrentToLast();
            _model.SelectedTranWrappedForDatagrid = (TranWrappedForDatagrid)_model.SortedRows.CurrentItem;
        }
    }
}