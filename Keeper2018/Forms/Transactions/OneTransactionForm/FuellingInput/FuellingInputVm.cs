using System;
using Caliburn.Micro;

namespace Keeper2018
{
    public class FuellingInputVm: PropertyChangedBase
    {
        public KeeperDb Db;

        private double _volume;
        private decimal _amount;
        private CurrencyCode _currency;
        private DateTime _timestamp;

        public DateTime Timestamp
        {
            get => _timestamp;
            set
            {
                if (value.Equals(_timestamp)) return;
                _timestamp = value;
                EvaluatePrices();
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(OneLitrePrice));
                NotifyOfPropertyChange(nameof(OneLitreInUsd));
            }
        }

        public double Volume
        {
            get => _volume;
            set
            {
                if (value.Equals(_volume)) return;
                _volume = value;
                EvaluatePrices();
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(OneLitrePrice));
                NotifyOfPropertyChange(nameof(OneLitreInUsd));
            }
        }

        public FuelType FuelType { get; set; }

        public decimal Amount
        {
            get => _amount;
            set
            {
                if (value == _amount) return;
                _amount = value;
                EvaluatePrices();
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(OneLitrePrice));
                NotifyOfPropertyChange(nameof(OneLitreInUsd));
            }
        }

        public CurrencyCode Currency
        {
            get => _currency;
            set
            {
                if (value == _currency) return;
                _currency = value;
                EvaluatePrices();
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(OneLitrePrice));
                NotifyOfPropertyChange(nameof(OneLitreInUsd));
                NotifyOfPropertyChange(nameof(CurrencyName));
            }
        }

        public string CurrencyName => _currency.ToString().ToLower();

        public string Comment { get; set; }

        public int CarAccountId { get; set; }

        public decimal OneLitrePrice { get; set; }
        public decimal OneLitreInUsd { get; set; }

       
        private void EvaluatePrices()
        {
            if (Db == null || _amount == 0 || Math.Abs(_volume) < 0.01) return;
            OneLitrePrice = _amount / (decimal)_volume;
            OneLitreInUsd = Db.AmountInUsd(Timestamp, _currency, _amount) / (decimal)_volume;
        }

    }
}
