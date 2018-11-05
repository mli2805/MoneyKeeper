using System;
using System.Collections.Generic;

namespace Keeper2018
{
    [Serializable]
    public class DepositOffer
    {
        public int Id { get; set; }
        public Account Bank { get; set; }
        public string Title { get; set; }

        // Conditions of offer could be changed (especially rates, initial sum) while Title remains the same
        // Conditions are applied from some date - key in dictionary
        public Dictionary<DateTime, DepositEssential> Essentials {get; set; }
        public string Comment { get; set; }
    }
}