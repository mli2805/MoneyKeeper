using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;

namespace Keeper2018.AmountInputControl
{
    class AmountInputControlVm : PropertyChangedBase
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

        private CurrencyCodes? _currency;
        public CurrencyCodes? Currency
        {
            get { return _currency; }
            set
            {
                if (value == _currency) return;
                _currency = value;
                NotifyOfPropertyChange();
            }
        }

        public List<CurrencyCodes> Currencies { get; set; } = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();


    }
}
