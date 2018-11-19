using System;
using System.Collections.Generic;

namespace Keeper2018
{
    public class DepositOfferModel
    {
        public int Id { get; set; }
        public Account Bank { get; set; }
        public string Title { get; set; }

        // Conditions of offer could be changed (especially rates, initial sum) while Title remains the same
        // Conditions are applied from some date - key in dictionary
        public Dictionary<DateTime, DepositEssential> Essentials {get; set; }
        public string Comment { get; set; }

        public DepositOfferModel DeepCopy()
        {
            var result = new DepositOfferModel();
            result.Id = Id;
            result.Bank = Bank;
            result.Title = Title;
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