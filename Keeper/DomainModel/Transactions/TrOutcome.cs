using System;
using System.Windows.Media;
using Keeper.ByFunctional.BalanceEvaluating.Ilya;

namespace Keeper.DomainModel.Transactions
{
    [Serializable]
    public class TrOutcome : TrBase
    {
        public override Brush TransactionFontColor => Brushes.Red;
        public override int SignForAmount(Account account)
        {
            return -1;
        }

        public override MoneyBag AmountForAccount(Account account)
        {
            return MyAccount.Is(account) ? new MoneyBag(new Money(Currency, -Amount)) : null;
        }
    }
}