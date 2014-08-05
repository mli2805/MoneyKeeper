using System;
using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace Keeper.DomainModel.Deposit
{
    [Serializable]
    public class BankDepositOffer
    {
        public int Id { get; set; }
        public Account BankAccount { get; set; }
        public string DepositTitle { get; set; }
        public CurrencyCodes Currency { get; set; }

        public BankDepositCalculatingRules CalculatingRules { get; set; }
        public ObservableCollection<DepositRateLine> RateLines { get; set; }

        public string Comment { get; set; }

        public BankDepositOffer()
        {
            var idGenerator = IoC.Get<DbIdGenerator>();
            Id = idGenerator.GenerateBankDepositOfferId();
            CalculatingRules = new BankDepositCalculatingRules();
            RateLines = new ObservableCollection<DepositRateLine>();
        }

        public BankDepositOffer(int id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return BankAccount.Name + " " + DepositTitle + " " + Currency;
        }
    }
}