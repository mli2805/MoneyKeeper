using System;

namespace KeeperDomain
{
    [Serializable]
    public class DepositOffer
    {
        public int Id { get; set; } //PK
        public string Title { get; set; }
        public bool IsNotRevocable { get; set; }
        public RateType RateType { get; set; }
        public bool IsAddLimited { get; set; }
        public int AddLimitInDays { get; set; }


        // при создании депозита или карточки эти поля возмутся из офера
        // а при создании банковского счета их надо будет выбрать руками
        public int BankId { get; set; } 
        public CurrencyCode MainCurrency { get; set; }
        //

        public Duration DepositTerm { get; set; }

        public string Comment { get; set; }

        public string Dump()
        {
            return Id + " ; " + BankId + " ; " + Title + " ; " + IsNotRevocable + " ; " + 
                   RateType + " ; " + IsAddLimited + " ; " + AddLimitInDays + " ; " + 
                   MainCurrency + " ; " + (DepositTerm?.Dump() ?? new Duration().Dump()) + " ; " + Comment;
        }
    }
}