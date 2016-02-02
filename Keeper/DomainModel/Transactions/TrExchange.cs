using System;
using System.Windows.Media;
using Keeper.ByFunctional.BalanceEvaluating.Ilya;

namespace Keeper.DomainModel.Transactions
{
    [Serializable]
    public class TrExchange : TrBase
    {
        private decimal _amountInReturn;
        private CurrencyCodes _currencyInReturn;
        public decimal AmountInReturn
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
        public override int SignForAmount(Account account)
        {
            return -1;
        }

        public override MoneyBag AmountForAccount(Account account)
        {
            if (MyAccount.Is(account))
                return new MoneyBag(new Money(CurrencyInReturn, AmountInReturn)) - new MoneyBag(new Money(Currency, -Amount));

            return null;
        }
    }
}