using System;

namespace KeeperDomain
{
    [Serializable]
    public class Deposit : IDumpable, IParsable<Deposit>
    {
        public int Id { get; set; } // совпадает с ID Account'a и BankAccount'a

        public bool IsAdditionsBanned { get; set; }

        public Deposit Clone()
        {
            return (Deposit)MemberwiseClone();
        }

        public string Dump()
        {
            return Id + " ; " + IsAdditionsBanned;
        }

        public Deposit FromString(string s)
        {
            var substrings = s.Split(';');
            Id = int.Parse(substrings[0].Trim());
            IsAdditionsBanned = bool.Parse(substrings[1].Trim());
            return this;
        }
    }


}