using System.Windows.Media;
using Caliburn.Micro;

namespace Keeper2018
{
    public class VerificationLine : PropertyChangedBase
    {
        private bool _isChecked;

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (value == _isChecked) return;
                _isChecked = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(BackgroundBrush));
            }
        }

        public decimal Amount { get; set; }
        public string AmountStr => Amount.ToString("#,0.##");
        public string Date { get; set; }
        public string Counterparty { get; set; }
        public string Text { get; set; }
        public OperationType OperationType { get; set; }
        public Brush ForegroundBrush => OperationType.FontColor();
        public Brush BackgroundBrush => IsChecked ? Brushes.LightGray : Brushes.White;
    }
}