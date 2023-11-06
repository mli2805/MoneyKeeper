using System;
using System.Collections.Generic;

namespace KeeperDomain
{
    [Serializable]
    public class ButtonCollection : IDumpable
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<int> AccountIds { get; set; } = new List<int>();

        public string Dump()
        {
            var result = Id + " ; " + Name;
            if (AccountIds != null && AccountIds.Count > 0)
            {
                var ids = string.Join(" ; ", AccountIds);
                result += " ; " + ids;
            }
            return result;
        }
    }
}