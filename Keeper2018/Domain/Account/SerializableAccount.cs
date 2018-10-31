using System;
using System.Collections.Generic;

namespace Keeper2018
{
    [Serializable]
    public class SerializableAccount
    {
        public int Id;
        public int OwnerId;
        public List<SerializableAccount> Children;
        public string Header;
        public bool IsExpanded;
    }
}