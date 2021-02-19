using System;
using System.Collections.Generic;
using KeeperDomain;

namespace Keeper2018
{
    public class DepositOfferModel
    {
        public int Id { get; set; }
        public AccountModel Bank { get; set; }
        public string Title { get; set; }
        public bool IsNotRevocable { get; set; }
        public CurrencyCode MainCurrency { get; set; }

        // Conditions of offer could be changed (especially rates, initial sum while Title remains the same)
        // only for newly opened deposits
        // Conditions are applied from some date - key in dictionary
        public Dictionary<DateTime, DepositConditions> ConditionsMap { get; set; }
        public string Comment { get; set; }

        public DepositOfferModel(int id)
        {
            Id = id;
            ConditionsMap = new Dictionary<DateTime, DepositConditions>();
        }

        public override string ToString()
        {
            return $"{Bank.Header} {Title} {MainCurrency}";
        }

        public DepositOfferModel DeepCopy()
        {
            var result = new DepositOfferModel(Id);
            result.Bank = Bank;
            result.Title = Title;
            result.IsNotRevocable = IsNotRevocable;
            result.MainCurrency = MainCurrency;
            result.ConditionsMap = new Dictionary<DateTime, DepositConditions>();
            foreach (var pair in ConditionsMap)
            {
                result.ConditionsMap.Add(pair.Key, pair.Value.DeepCopy());
            }
            result.Comment = Comment;
            return result;
        }
    }
}