using System;

namespace Keeper.DomainModel.Transactions
{
    [Serializable]
    public class TrForex : TrExchangeWithTransfer
    {
        private int _code;
        private bool _isLastInChain;

        public int Code
        {
            get { return _code; }
            set
            {
                if (value == _code) return;
                _code = value;
                NotifyOfPropertyChange();
            }
        }
        public bool IsLastInChain
        {
            get { return _isLastInChain; }
            set
            {
                if (value == _isLastInChain) return;
                _isLastInChain = value;
                NotifyOfPropertyChange();
            }
        }
    }
}