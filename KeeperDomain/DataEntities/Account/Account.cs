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

        public int AssociatedIncomeId { get; set; } // for external
        public int AssociatedExpenseId { get; set; } // for external
        public int AssociatedExternalId { get; set; } // for tag

        public string ButtonName { get; set; } // face of shortcut button (if exists)

        public string Comment { get; set; }

        public string Dump(int level)
        {
            var shiftedName = new string(' ', level * 2) + Header;
            return Id + " ; " + shiftedName + " ; " + OwnerId + " ; " + IsExpanded + " ; " + 
                   AssociatedIncomeId + " ; " + AssociatedExpenseId + " ; " + AssociatedExternalId + " ; " + 
                   ButtonName + " ; " + 
                   (Comment?.Replace("\r\n", "|") ?? "");
        }

    }
}