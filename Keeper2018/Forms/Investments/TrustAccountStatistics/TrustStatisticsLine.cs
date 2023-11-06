using System.Windows.Media;
using Caliburn.Micro;

namespace Keeper2018
{
    public class TrustStatisticsLine : PropertyChangedBase
    {
        public TrustTranModel Tran { get; set; }

        public string Title { get; set; }
        public string AmountIn { get; set; }
        public string AmountOut { get; set; }
        public string BynFee { get; set; }
        public decimal BalanceAfter { get; set; }
        public string BalanceAfterStr => IsForWholeTrustAccount
            ? $"{BalanceAfter:#,0.00} {Tran.TrustAccount.Currency.ToString().ToLowerInvariant()}"
            : $"{BalanceAfter:#,0.00} шт";

        public bool IsForWholeTrustAccount;

        public Brush TransBrush { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value.Equals(_isSelected)) return;
                _isSelected = value;
                NotifyOfPropertyChange(() => IsSelected);
            }
        }

    }
}