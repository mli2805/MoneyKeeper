using System;
using System.ComponentModel.DataAnnotations;

namespace KeeperDomain
{
    [Serializable]
    public class Account : IDumpable, IParsable<Account>
    {
        public int Id { get; set; }
        [MaxLength(50)] public string Name { get; set; }
        public int ParentId { get; set; }

        public bool IsFolder;
        public bool IsExpanded;

        public int AssociatedIncomeId { get; set; } // for counterparty
        public int AssociatedExpenseId { get; set; } // for counterparty
        public int AssociatedExternalId { get; set; } // for category
        public int AssociatedTagId { get; set; } // for counterparty or category

        [MaxLength(20)] public string ShortName { get; set; }
        [MaxLength(5)] public string ButtonName { get; set; } // face of shortcut button (if exists)

        [MaxLength(100)] public string Comment { get; set; }

        public string Dump()
        {
            return Id.ToString().PadLeft(4) + " ; " + Name + " ; " + ParentId + " ; " + IsFolder + " ; " + IsExpanded + " ; " + 
                   AssociatedIncomeId + " ; " + AssociatedExpenseId + " ; " + AssociatedExternalId + " ; " + AssociatedTagId + " ; " + 
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
            AssociatedTagId = int.Parse(substrings[8]);
            ShortName = substrings[9].Trim();
            ButtonName = substrings[10].Trim();
            Comment = substrings[11].Trim();
            return this;
        }
    }
}