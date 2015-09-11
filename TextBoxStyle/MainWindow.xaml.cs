using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using TextBoxStyle.Annotations;

namespace TextBoxStyle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private decimal _amount;
        private CurrencyCodesWrapper _currentCurrencyCodesWrapper;
        private CurrencyCodes _currentCurrency;

        public decimal Amount
        {
            get { return _amount; }
            set
            {
                if (value == _amount) return;
                _amount = value;
                OnPropertyChanged();
            }
        }

        public List<ComboboxItem> CurrencyList { get; set; }

        public CurrencyCodes CurrentCurrency
        {
            get { return _currentCurrency; }
            set
            {
                if (value == _currentCurrency) return;
                _currentCurrency = value;
                OnPropertyChanged();
            }
        }

        public CurrencyCodesWrapper CurrentCurrencyCodesWrapper
        {
            get { return _currentCurrencyCodesWrapper; }
            set
            {
                _currentCurrencyCodesWrapper = value;
                if (value.Currency == CurrencyCodes.BYR)
                {
                    AmountTextBoxByr.Visibility = Visibility.Visible;
                    AmountTextBoxOther.Visibility = Visibility.Hidden;
                }
                else
                {
                    AmountTextBoxByr.Visibility = Visibility.Hidden;
                    AmountTextBoxOther.Visibility = Visibility.Visible;
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            Amount = 3.04m;
            CurrencyList = new List<ComboboxItem>()
            {
                new ComboboxItem(){Currency = CurrencyCodes.BYR, CurrencyName = "BYR"},
                new ComboboxItem(){Currency = CurrencyCodes.RUB, CurrencyName = "RUB"},
                new ComboboxItem(){Currency = CurrencyCodes.USD, CurrencyName = "USD"},
                new ComboboxItem(){Currency = CurrencyCodes.EUR, CurrencyName = "EUR"}
            };

            this.DataContext = this;

            CurrentCurrencyCodesWrapper = new CurrencyCodesWrapper() {Currency = CurrencyCodes.BYR};

        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
