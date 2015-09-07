using System;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using Keeper.ByFunctional.DepositProcessing;

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
        public ObservableCollection<BankDepositRateLine> RateLines { get; set; }

        public string Comment { get; set; }

        public BankDepositOffer()
        {
            var idGenerator = IoC.Get<DepositOfferIdGenerator>();
            Id = idGenerator.GenerateNewBankDepositOfferId();
            Currency = CurrencyCodes.BYR;
            CalculatingRules = new BankDepositCalculatingRules();
            RateLines = new ObservableCollection<BankDepositRateLine>();
        }

        public BankDepositOffer(int id)
        {
            Id = id;
        }

        public bool IsInvalid()
        {
            return (BankAccount == null || DepositTitle == null || Currency == 0);
        }
        public override string ToString()
        {
            if (IsInvalid()) return "ввод не окончен";
            return BankAccount.Name + " " + DepositTitle + " " + Currency;
        }
    }
}