using System;

namespace KeeperDomain
{
    [Serializable]
    public class Account : IDumpable, IParsable<Account>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }

        public bool IsFolder;
        public bool IsExpanded;

        public int AssociatedIncomeId { get; set; } // for external
        public int AssociatedExpenseId { get; set; } // for external
        public int AssociatedExternalId { get; set; } // for tag

        public string ShortName { get; set; }
        public string ButtonName { get; set; } // face of shortcut button (if exists)

        public string Comment { get; set; }

        public string Dump()
        {
            return Id + " ; " + Name + " ; " + ParentId + " ; " + IsFolder + " ; " + IsExpanded + " ; " + 
                   AssociatedIncomeId + " ; " + AssociatedExpenseId + " ; " + AssociatedExternalId + " ; " + 
                   ShortName + " ; " + ButtonName + " ; " + 
                   (Comment?.Replace("\r\n", "|") ?? "");
        }

        public Account FromString(string s)
        {
            var substrings = s.Split(';');
            Id = int.Parse(substrings[0]);
            Name = substrings[1].Trim();
            ParentId = int.Parse(substrings[2]);
            IsFolder = Convert.ToBoolean(substrings[3]);
            IsExpanded = Convert.ToBoolean(substrings[4]);
            AssociatedIncomeId = int.Parse(substrings[5]);
            AssociatedExpenseId = int.Parse(substrings[6]);
            AssociatedExternalId = int.Parse(substrings[7]);
            ShortName = substrings[8].Trim();
            ButtonName = substrings[9].Trim();
            Comment = substrings[10].Trim();
            return this;
        }
    }
}