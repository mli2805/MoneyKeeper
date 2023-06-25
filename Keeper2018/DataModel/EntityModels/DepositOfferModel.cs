using System;
using System.Collections.Generic;
using System.Windows.Media;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class DepositOfferModel : PropertyChangedBase
    {
        public int Id { get; set; }
        public AccountModel Bank { get; set; }
        public string Title { get; set; }
        public bool IsNotRevocable { get; set; }
        public string NotRevocableStr => IsNotRevocable ? "безотзыв" : "отзывной";

        private RateType _rateType;
        public RateType RateType
        {
            get => _rateType;
            set
            {
                if (value == _rateType) return;
                _rateType = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isAddLimited;
        public bool IsAddLimited
        {
            get => _isAddLimited;
            set
            {
                if (value == _isAddLimited) return;
                _isAddLimited = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(AddLimitStr));
            }
        }

        public int AddLimitInDays { get; set; }

        public string AddLimitStr => IsAddLimited
            ? AddLimitInDays == 0
                ? "не предусмотрены" 
                : $"первые {AddLimitInDays} дней"
            : "без ограничений";
        public CurrencyCode MainCurrency { get; set; }

        public DurationModel DepositTerm { get; set; }


        public Brush BackgroundColor { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (value == _isSelected) return;
                _isSelected = value;
                NotifyOfPropertyChange();
            }
        }


        // Conditions of offer could be changed (especially rates, initial sum while Title remains the same)
        // only for newly opened deposits
        // Conditions are applied from some date - key in dictionary
        public Dictionary<DateTime, DepoCondsModel> CondsMap { get; private set; } = new Dictionary<DateTime, DepoCondsModel>();
        public string Comment { get; set; }

        public override string ToString()
        {
            return $"{Bank.Header} {Title} {MainCurrency}";
        }

        public DepositOfferModel DeepCopyExceptBank()
        {
            var result = new DepositOfferModel
            {
                Id = Id,
                Bank = Bank,
                Title = Title,
                IsNotRevocable = IsNotRevocable,
                RateType = RateType,
                IsAddLimited = IsAddLimited,
                AddLimitInDays = AddLimitInDays,
                MainCurrency = MainCurrency,
                DepositTerm = DepositTerm.Clone(),
                CondsMap = new Dictionary<DateTime, DepoCondsModel>(),
                Comment = Comment,
                BackgroundColor = BackgroundColor,
            };
            foreach (var pair in CondsMap)
            {
                result.CondsMap.Add(pair.Key, pair.Value.DeepCopyBin());
            }
            return result;
        }
    }
}