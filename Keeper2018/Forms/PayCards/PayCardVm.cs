using System;
using KeeperDomain;

namespace Keeper2018.PayCards
{
    public class PayCardVm
    {
        public string CardNumber { get; set; }
        public string CardHolder { get; set; }
        public int CardOwner { get; set; }

        public PaymentSystem PaymentSystem { get; set; }
        public bool IsPayPass { get; set; }

        public string AgreementNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
        public string Comment { get; set; }

        public AccountModel BankAccount { get; set; }
        public CurrencyCode MainCurrency { get; set; }

        public string Name { get; set; }

        public decimal Amount { get; set; }
    }
}
