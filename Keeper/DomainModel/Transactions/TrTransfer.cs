using System.Windows.Media;

namespace Keeper.DomainModel.Transactions
{
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
    }
}