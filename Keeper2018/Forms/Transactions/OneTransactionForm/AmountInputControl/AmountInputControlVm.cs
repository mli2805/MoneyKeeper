using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;

namespace Keeper2018
{
    public class AmountInputControlVm : PropertyChangedBase
    {
        public string LabelContent { get; set; }
        public Brush AmountColor { get; set; } 

        private decimal _amount;
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                if (value == _amount) return;
                _amount = value;
                NotifyOfPropertyChange();
            }
        }

        private CurrencyCode _currency;
        public CurrencyCode Currency
        {
            get { return _currency; }
            set
            {
                if (value == _currency) return;
                _currency = value;
                NotifyOfPropertyChange();
            }
        }

        public List<CurrencyCode> Currencies { get; set; } = Enum.GetValues(typeof(CurrencyCode)).OfType<CurrencyCode>().ToList();


    }
}
