using System;

namespace Keeper2018
{
    [Serializable]
    public class Account
    {
        public int Id;
        public string Header;
        public int OwnerId;
        public bool IsExpanded;
        public bool IsFolder;
        public Deposit Deposit;
        public PayCard Card;

        public bool IsDeposit => Deposit != null;
        public bool IsCard => Card != null;
        public string Name
        {
            get => Header;
            set => Header = value;
        }

        public override string ToString() => Name;
        public string Comment;
        
        public string Dump(int level)
        {
            var shiftedName = new string(' ', level * 2) + Name;
            return Id + " ; " + shiftedName + " ; " + OwnerId + " ; " +
                   IsFolder + " ; " + IsExpanded + " ; " + Comment;
        }

    }
}