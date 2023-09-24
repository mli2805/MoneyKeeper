using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class PayCardFilterVm : PropertyChangedBase, IDataErrorInfo
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
                NotifyOfPropertyChange();
            }
        }

        public bool OwnerMine
        {
            get => _ownerMine;
            set
            {
                _ownerMine = value;
                NotifyOfPropertyChange();
            }
        }

        public bool OwnerYuliya
        {
            get => _ownerYuliya;
            set
            {
                _ownerYuliya = value;
                 NotifyOfPropertyChange();
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

        private string _balanceStr = "0";
        public string BalanceStr
        {
            get => _balanceStr;
            set
            {
                if (value == _balanceStr) return;
                _balanceStr = value;
                NotifyOfPropertyChange();
            }
        }

        public double Balance { get; set; }

        public PayCardFilterVm()
        {
            SelectedCurrency = Currencies.First();
        }

        public bool Allow(PayCardVm accountItemModel)
        {
            if (_ownerYuliya && accountItemModel.IsMine) return false;
            if (_ownerMine && !accountItemModel.IsMine) return false;

            if (_selectedCurrency != "все" && _selectedCurrency != accountItemModel.MainCurrency.ToString().ToLower()) return false;

            if ((double)accountItemModel.Amount < Balance) return false;

            return true;
        }

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "BalanceStr":
                        var tryParse = double.TryParse(_balanceStr, out double bal);
                        if (tryParse)
                        {
                            Balance = bal;
                            NotifyOfPropertyChange(nameof(Balance));
                        }
                        return tryParse ? string.Empty : "Failed";
                    default: return string.Empty;
                }
            }
        }

        public string Error { get; } = null;

        public void ClearAll()
        {
            BalanceStr = "0";
            SelectedCurrency = "все";
            OwnerAll = true;
        }

        public void ShowAllByn5()
        {
            BalanceStr = "5.01";
            SelectedCurrency = "byn";
            OwnerAll = true;
        }
        
        public void ShowMineByn5()
        {
            BalanceStr = "5.01";
            SelectedCurrency = "byn";
            OwnerMine = true;
        }
    }
}
