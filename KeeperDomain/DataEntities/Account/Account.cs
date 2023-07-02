using System;

namespace KeeperDomain
{
    [Serializable]
    public class Account
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }

        public bool IsFolder;
        public bool IsExpanded;

        public int BankId { get; set; } // 0 if not in bank

        public int AssociatedIncomeId { get; set; } // for external
        public int AssociatedExpenseId { get; set; } // for external
        public int AssociatedExternalId { get; set; } // for tag

        public string ButtonName { get; set; } // face of shortcut button (if exists)

        public string Comment { get; set; }

        public string Dump(int level)
        {
            var shiftedName = new string(' ', level * 2) + Name;
            return Id + " ; " + shiftedName + " ; " + ParentId + " ; " + IsFolder + " ; " + IsExpanded + " ; " + BankId + " ; " + 
                   AssociatedIncomeId + " ; " + AssociatedExpenseId + " ; " + AssociatedExternalId + " ; " + 
                   ButtonName + " ; " + 
                   (Comment?.Replace("\r\n", "|") ?? "");
        }

    }
}