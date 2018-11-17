using System;

namespace Keeper2018
{
    [Serializable]
    public class Deposit
    {
        public int MyAccount;
        public int DepositOffer;
        public string Serial;
        public DateTime StartDate;
        public DateTime FinishDate;
        public string Comment;
        public string ShortName;
    }
}