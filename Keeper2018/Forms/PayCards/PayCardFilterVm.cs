using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class PayCardFilterVm : PropertyChangedBase
    {
        #region owner

        private bool _ownerAll = true;
        private bool _ownerMine;
        private bool _ownerYuliya;

        public bool OwnerAll
        {
            get => _ownerAll;
            set
            {
                _ownerAll = value;
                if (!value) NotifyOfPropertyChange();
            }
        }

        public bool OwnerMine
        {
            get => _ownerMine;
            set
            {
                _ownerMine = value;
                if (!value) NotifyOfPropertyChange();
            }
        }

        public bool OwnerYuliya
        {
            get => _ownerYuliya;
            set
            {
                _ownerYuliya = value;
                if (!value) NotifyOfPropertyChange();
            }
        }


        #endregion


        public List<string> Currencies { get; set; } = new List<string>() { "все", "byn", "usd", "euro", "rur", "cny" };

        private string _selectedCurrency;

        public string SelectedCurrency
        {
            get => _selectedCurrency;
            set
            {
                if (value == _selectedCurrency) return;
                _selectedCurrency = value;
                NotifyOfPropertyChange();
            }
        }

        public string BalanceStr { get; set; } = "0";

        private double _balance;
        public double Balance   
        {
            get => _balance;
            set
            {
                if (value.Equals(_balance)) return;
                _balance = value;
                NotifyOfPropertyChange();
            }
        }

        public PayCardFilterVm()
        {
            SelectedCurrency = Currencies.First();
        }
    }
}
