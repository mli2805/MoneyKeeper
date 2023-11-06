using System;
using System.Collections.Generic;

namespace KeeperDomain
{
    [Serializable]
    public class ButtonCollection : IDumpable, IParsable<ButtonCollection>
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

        public ButtonCollection FromString(string s)
        {
            var substrings = s.Split(';');
            Id = int.Parse(substrings[0]);
            Name = substrings[1];
            for (int i = 2; i < substrings.Length; i++)
            {
                AccountIds.Add(int.Parse(substrings[i]));
            }
            return this;
        }
    }
}