using System.ComponentModel;
using System.Runtime.CompilerServices;
using TextBoxStyle.Annotations;

namespace TextBoxStyle
{
    public enum CurrencyCodes
    {
        BYR,
        RUB,
        USD,
        EUR
    }

    public class CurrencyCodesWrapper : INotifyPropertyChanged
    {
        private CurrencyCodes _currency;

        public CurrencyCodes Currency
        {
            get { return _currency; }
            set
            {
                if (value == _currency) return;
                _currency = value;
                OnPropertyChanged("Currency");
            }
        }

        public CurrencyCodesWrapper()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ComboboxItem
    {
        public CurrencyCodes Currency { get; set; }
        public string CurrencyName { get; set; }
    }
}
