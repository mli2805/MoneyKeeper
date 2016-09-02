using Keeper.DomainModel.Trans;

namespace Keeper.ViewModels.TransWithTags
{
    public class TranFilter
    {
        private TranWrappedForDatagrid _wrappedTran;
        private FilterModel _filterModel;
        public bool Filter(TranWrappedForDatagrid wrappedTran, FilterModel filterModel)
        {
            _wrappedTran = wrappedTran;
            if (filterModel == null) return true;
            _filterModel = filterModel;
            return FilterOperationType() && FilterAccount() && FilterAmount() && FilterCurrency() && FilterComment();
        }

        private bool FilterOperationType()
        {
            if (_filterModel.MyOperationType == null || !_filterModel.MyOperationType.IsOn) return true;
            return _filterModel.MyOperationType.Operation == _wrappedTran.Tran.Operation;
        }
        private bool FilterAccount()
        {
            if (_filterModel.MyAccName == null) return true;
            if (_filterModel.IsAccNamePosition1) return _wrappedTran.Tran.MyAccount.Is(_filterModel.MyAccName.Name);
            if (_filterModel.IsAccNamePosition2) return _wrappedTran.Tran.MySecondAccount != null && _wrappedTran.Tran.MySecondAccount.Is(_filterModel.MyAccName.Name);
            // if (_filterModel.IsAccNamePosition12)
            return _wrappedTran.Tran.MyAccount.Is(_filterModel.MyAccName.Name) ||
                   _wrappedTran.Tran.MySecondAccount != null && _wrappedTran.Tran.MySecondAccount.Is(_filterModel.MyAccName.Name);
        }

        private bool FilterAmount()
        {
            if (_filterModel.Amount == null) return true;
            if (_filterModel.AmountEqualTo) return decimal.Parse(_filterModel.Amount) == _wrappedTran.Tran.Amount;
            if (_filterModel.AmountLessThan) return decimal.Parse(_filterModel.Amount) > _wrappedTran.Tran.Amount;
            // if (_filterModel.AmountGreaterThan) 
            return decimal.Parse(_filterModel.Amount) < _wrappedTran.Tran.Amount;
        }

        private bool FilterCurrency()
        {
            if (_filterModel.MyCurrency == null || !_filterModel.MyCurrency.IsOn) return true;
            if (_filterModel.IsCurrencyPosition1) return _wrappedTran.Tran.Currency == _filterModel.MyCurrency.Currency;
            if (_filterModel.IsCurrencyPosition2) return _wrappedTran.Tran.CurrencyInReturn != null && _wrappedTran.Tran.CurrencyInReturn == _filterModel.MyCurrency.Currency;
            // if (_filterModel.IsCurrencyPosition12)
            return _wrappedTran.Tran.Currency == _filterModel.MyCurrency.Currency ||
                   _wrappedTran.Tran.CurrencyInReturn != null && _wrappedTran.Tran.CurrencyInReturn == _filterModel.MyCurrency.Currency;
        }

        private bool FilterComment()
        {
            if (_filterModel.MyComment == null) return true;
            return _wrappedTran.Tran.Comment.Contains(_filterModel.MyComment);
        }

    }
}