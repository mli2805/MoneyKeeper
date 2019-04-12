using System;

namespace Keeper2018
{
    [Serializable]
    public class PayCard
    {
        public int MyAccountId;

        public int LastFourDigits;
        public CurrencyCode MainCurrency;
        public DateTime StartDate;
        public DateTime FinishDate;
        public PaymentSystem PaymentSystem;
        public bool IsPayPass;

        public string Name;
        public string Comment;

        public string Dump()
        {
            return  MyAccountId + " ; " + LastFourDigits + " ; " + MainCurrency +" ; " + 
                    $"{StartDate.Date:dd/MM/yyyy}" + " ; " + $"{FinishDate.Date:dd/MM/yyyy}" + " ; " + 
                    PaymentSystem + " ; " + IsPayPass +" ; " + 
                    Name + " ; " + (Comment?.Replace("\r\n", "|") ?? "");
        }

    }
}