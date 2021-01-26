using System;

namespace KeeperDomain
{
    [Serializable]
    public class Account
    {
        public int Id { get; set; }
        public string Header { get; set; }
        public int OwnerId { get; set; }
        public bool IsExpanded;
        public bool IsFolder { get; set; }
        public Deposit Deposit { get; set; }

        public bool IsDeposit => Deposit != null;
        public bool IsCard => Deposit?.Card != null;
        public string Name
        {
            get => Header;
            
        }

        public override string ToString() => Name;
        public string Comment { get; set; }

        public string Dump(int level)
        {
            var shiftedName = new string(' ', level * 2) + Name;
            return Id + " ; " + shiftedName + " ; " + OwnerId + " ; " +
                   IsFolder + " ; " + IsExpanded + " ; " + (Comment?.Replace("\r\n", "|") ?? "");
        }

    }
}