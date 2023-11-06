using System;

namespace KeeperDomain
{
    [Serializable]
    public class Deposit : IDumpable
    {
        public int Id { get; set; } // совпадает с ID Account'a и BankAccount'a
       
        public bool IsAdditionsBanned { get; set; }

        public string Dump()
        {
            return Id + " ; " + IsAdditionsBanned;
        }

    }


}