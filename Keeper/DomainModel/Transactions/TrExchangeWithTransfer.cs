using System;
using Keeper.ByFunctional.BalanceEvaluating.Ilya;

namespace Keeper.DomainModel.Transactions
{
    [Serializable]
    public class TrExchangeWithTransfer : TrExchange
    {
        private Account _mySecondAccount;
        public Account MySecondAccount
        {
            get { return _mySecondAccount; }
            set
            {
                if (Equals(value, _mySecondAccount)) return;
                _mySecondAccount = value;
                NotifyOfPropertyChange();
            }
        }
        public new string AccountForDatagrid => $"{MyAccount} ->\n  {MySecondAccount}";

        public override MoneyBag AmountForAccount(Account account)
        {
            var result = new MoneyBag();
            if (MyAccount.Is(account))
                result = result - new MoneyBag(new Money(Currency, -Amount));
            if (MySecondAccount.Is(account))
                result = result + new MoneyBag(new Money(CurrencyInReturn, AmountInReturn));
            return result;
        }

    }
}