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
        public Dictionary<DateTime, DepoCondsModel> CondsMap { get; private set; } = new Dictionary<DateTime, DepoCondsModel>();
        public string Comment { get; set; }

        public override string ToString()
        {
            return $"{Bank.Header} {Title} {MainCurrency}";
        }

        public DepositOfferModel DeepCopy()
        {
            var result = new DepositOfferModel
            {
                Id = Id,
                Bank = Bank,
                Title = Title,
                IsNotRevocable = IsNotRevocable,
                MainCurrency = MainCurrency,
                CondsMap = new Dictionary<DateTime, DepoCondsModel>(),
                Comment = Comment
            };
            foreach (var pair in CondsMap)
            {
                result.CondsMap.Add(pair.Key, pair.Value.DeepCopy());
            }
            return result;
        }
    }
}