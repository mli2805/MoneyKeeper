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

        // public override string ToString() => Header;
        public string Comment { get; set; }

        public string Dump(int level)
        {
            var shiftedName = new string(' ', level * 2) + Header;
            return Id + " ; " + shiftedName + " ; " + OwnerId + " ; " +
                   IsExpanded + " ; " + (Comment?.Replace("\r\n", "|") ?? "");
        }

    }
}