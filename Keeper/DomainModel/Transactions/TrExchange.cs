using System.Windows.Media;

namespace Keeper.DomainModel.Transactions
{
    public class TrExchange : TrBase
    {
        private double _amountInReturn;
        private CurrencyCodes _currencyInReturn;
        public double AmountInReturn
        {
            get { return _amountInReturn; }
            set
            {
                if (value.Equals(_amountInReturn)) return;
                _amountInReturn = value;
                NotifyOfPropertyChange();
            }
        }
        public CurrencyCodes CurrencyInReturn
        {
            get { return _currencyInReturn; }
            set
            {
                if (value == _currencyInReturn) return;
                _currencyInReturn = value;
                NotifyOfPropertyChange();
            }
        }
        public override Brush TransactionFontColor => Brushes.Green; 

//        public new string AmountForDatagrid => $"{Amount:0,0.00} {Currency} ->\n  {AmountInReturn:0,0.00} {CurrencyInReturn}";
        public new string AmountForDatagrid => ShowAmount(Amount, Currency) + " ->\n  " + ShowAmount(AmountInReturn, CurrencyInReturn);
    }
}