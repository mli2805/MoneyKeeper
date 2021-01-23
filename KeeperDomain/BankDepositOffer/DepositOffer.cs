using System;
using System.Collections.Generic;

namespace KeeperDomain
{
    [Serializable]
    public class DepositOffer
    {
        public int Id { get; set; } //PK
        public int BankId { get; set; }
        public string Title { get; set; }
        public bool IsNotRevocable { get; set; }
        public CurrencyCode MainCurrency { get; set; }

        // Conditions of offer could be changed (especially rates, initial sum while Title remains the same)
        // only for newly opened deposits
        // Conditions are applied from some date - key in dictionary
        public Dictionary<DateTime, DepositConditions> ConditionsMap {get; set; } = new Dictionary<DateTime, DepositConditions>();
        public string Comment { get; set; }

        public string Dump()
        {
            return Id + " ; " + BankId + " ; " + Title + " ; " + IsNotRevocable + " ; " + MainCurrency + " ; " + Comment;
        }
    }
}