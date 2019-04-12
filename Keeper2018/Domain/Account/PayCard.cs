using System;

namespace Keeper2018
{
    [Serializable]
    public class PayCard
    {
        public int MyAccountId;
        public int BankId;
        
        public string CardNumber;
        public string ContractNumber;
        public CurrencyCode MainCurrency;
        public DateTime StartDate;
        public DateTime FinishDate;
        public PaymentSystem PaymentSystem;
        public bool IsPayPass;

        public string Name;
        public string Comment;

        public string Dump()
        {
            return  MyAccountId + " ; " + CardNumber + " ; " + ContractNumber + " ; " + MainCurrency +" ; " + 
                    $"{StartDate.Date:dd/MM/yyyy}" + " ; " + $"{FinishDate.Date:dd/MM/yyyy}" + " ; " + 
                    PaymentSystem + " ; " + IsPayPass +" ; " + 
                    Name + " ; " + BankId + " ; " + (Comment?.Replace("\r\n", "|") ?? "");
        }

    }
}