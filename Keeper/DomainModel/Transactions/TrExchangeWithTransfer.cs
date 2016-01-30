namespace Keeper.DomainModel.Transactions
{
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
    }
}