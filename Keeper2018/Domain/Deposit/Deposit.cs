using System;

namespace Keeper2018
{
    [Serializable]
    public class Deposit
    {
        public int MyAccountId;
        public int DepositOfferId;
        public string Serial;
        public DateTime StartDate;
        public DateTime FinishDate;
        public string Comment;
        public string ShortName;
    }

  
}