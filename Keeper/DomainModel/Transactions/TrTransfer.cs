using System;
using System.Windows.Media;
using Keeper.ByFunctional.BalanceEvaluating.Ilya;

namespace Keeper.DomainModel.Transactions
{
    [Serializable]
    public class TrTransfer : TrBase
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
        public override Brush TransactionFontColor => Brushes.Black;

        public new string AccountForDatagrid => $"{MyAccount} ->\n  {MySecondAccount}";
        public override int SignForAmount(Account account)
        {
            return -1;
        }

        public override MoneyBag AmountForAccount(Account account)
        {
            if (MyAccount.Is(account))
            {
                if (MySecondAccount.Is(account)) return null;
                else return new MoneyBag(new Money(Currency, -Amount));
            }
            else
            {
                if (MySecondAccount.Is(account)) return new MoneyBag(new Money(Currency, Amount));
                else return null;
            }
        }
    }
}