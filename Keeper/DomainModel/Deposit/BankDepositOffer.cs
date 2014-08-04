using System;
using System.Collections.ObjectModel;
using System.Composition;
using Caliburn.Micro;

namespace Keeper.DomainModel
{
    [Serializable]
    public class BankDepositOffer
    {
        public int Id { get; set; }
        public Account BankAccount { get; set; }
        public string DepositTitle { get; set; }
        public CurrencyCodes Currency { get; set; }

        public DepositProcentsCalculatingRules CalculatingRules { get; set; }
        public ObservableCollection<DepositRateLine> RateLines { get; set; }

        public string Comment { get; set; }

        public BankDepositOffer()
        {
            var idGenerator = IoC.Get<DbIdGenerator>();
            Id = idGenerator.GenerateBankDepositOfferId();
            CalculatingRules = new DepositProcentsCalculatingRules();
            RateLines = new ObservableCollection<DepositRateLine>();
        }
    }
}