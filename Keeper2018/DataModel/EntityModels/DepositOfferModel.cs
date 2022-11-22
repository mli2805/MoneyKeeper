using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class DepositOfferModel : PropertyChangedBase
    {
        private RateType _rateType;
        private bool _isAddLimited;
        private string _rateFormula;
        public int Id { get; set; }
        public AccountModel Bank { get; set; }
        public string Title { get; set; }
        public bool IsNotRevocable { get; set; }
        public string NotRevocableStr => IsNotRevocable ? "безотзыв" : "отзывной";

        public RateType RateType
        {
            get => _rateType;
            set
            {
                if (value == _rateType) return;
                _rateType = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(RateFormulaVisibility));
            }
        }

        public Visibility RateFormulaVisibility =>
            RateType == RateType.Linked ? Visibility.Visible : Visibility.Collapsed;

        public string RateFormula
        {
            get => _rateFormula;
            set
            {
                if (value == _rateFormula) return;
                _rateFormula = value;
                NotifyOfPropertyChange();
            }
        }

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
            ? $"первые {AddLimitInDays} дней"
            : "без ограничений";
        public CurrencyCode MainCurrency { get; set; }

        public DurationModel DepositTerm { get; set; }

       
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
                RateFormula = RateFormula,
                IsAddLimited = IsAddLimited,
                AddLimitInDays = AddLimitInDays,
                MainCurrency = MainCurrency,
                DepositTerm = DepositTerm.Clone(),
                CondsMap = new Dictionary<DateTime, DepoCondsModel>(),
                Comment = Comment
            };
            foreach (var pair in CondsMap)
            {
                result.CondsMap.Add(pair.Key, pair.Value.DeepCopyBin());
            }
            return result;
        }
    }
}