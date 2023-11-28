using System;
using KeeperDomain;

namespace Keeper2018
{
    public class CardOrAccountVm
    {
        public string CardNumber { get; set; }
        public string CardHolder { get; set; }
        public bool IsMine { get; set; } // 0 - mine, 1 - julia

        public PaymentSystem PaymentSystem { get; set; } = PaymentSystem.CurrentAccount; // will be changed later, if necessary
        public string PaymentSystemStr => PaymentSystem == PaymentSystem.CurrentAccount ? " текущий счет" : PaymentSystem.ToString();
        public bool IsVirtual { get; set; }
        public bool IsPayPass { get; set; }

        public string AgreementNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }

        public AccountItemModel AccountItemOfBank { get; set; }
        public CurrencyCode MainCurrency { get; set; }

        public string Name { get; set; }

        public decimal Amount { get; set; }
    }
}
