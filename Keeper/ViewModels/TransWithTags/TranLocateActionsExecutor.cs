using System.Linq;

namespace Keeper.ViewModels.TransWithTags
{
    public class TranLocateActionsExecutor
    {
        private TransModel _model;

        public void Do(TranAction action, TransModel model)
        {
            _model = model;
            switch (action)
            {
                case TranAction.GoToDate: GoToDate(); return ;
                case TranAction.GoToEnd: GoToEnd(); return ;
                case TranAction.Filter:
                    return ;
                default:
                    return ;
            }
        }
        private void GoToDate()
        {
            var askedTran = _model.Rows.FirstOrDefault(t => t.Tran.Timestamp >= _model.AskedDate);
            if (askedTran != null) _model.SelectedTranWrappedForDatagrid = askedTran;
        }

        private void GoToEnd()
        {
            _model.SelectedTranWrappedForDatagrid = _model.Rows.Last();
        }
    }
}