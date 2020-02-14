using System;
using System.Collections.Generic;

namespace Keeper2018
{
    public class DepositOfferModel
    {
        public int Id { get; set; }
        public Account Bank { get; set; }
        public string Title { get; set; }
        public bool IsNotRevocable { get; set; }
        public CurrencyCode MainCurrency { get; set; }

        // Conditions of offer could be changed (especially rates, initial sum) while Title remains the same
        // Conditions are applied from some date - key in dictionary
        public Dictionary<DateTime, DepositEssential> Essentials {get; set; }
        public string Comment { get; set; }

        public DepositOfferModel(int id)
        {
            Id = id;
            Essentials = new Dictionary<DateTime, DepositEssential>() { { DateTime.Today, new DepositEssential() { DepositOfferId = Id } } };
        }

        public override string ToString()
        {
            return $"{Bank.Header} {Title} {MainCurrency.ToString()}";
        }

        public DepositOfferModel DeepCopy()
        {
            var result = new DepositOfferModel(Id);
            result.Bank = Bank;
            result.Title = Title;
            result.IsNotRevocable = IsNotRevocable;
            result.MainCurrency = MainCurrency;
            result.Essentials = new Dictionary<DateTime, DepositEssential>();
            foreach (var pair in Essentials)
            {
                result.Essentials.Add(pair.Key, pair.Value.DeepCopy());
            }
            result.Comment = Comment;
            return result;
        }
    }
}