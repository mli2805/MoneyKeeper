using System;
using System.Collections.ObjectModel;

namespace Keeper.DomainModel
{
    [Serializable]
    public class BankDepositOffer
    {
        public Account BankAccount { get; set; }
        public string DepositTitle { get; set; }
        public CurrencyCodes Currency { get; set; }

        public DepositProcentsCalculatingRules CalculatingRules { get; set; }
        public ObservableCollection<DepositRateLine> RateLines { get; set; }

        public string Comment { get; set; }

    }
}