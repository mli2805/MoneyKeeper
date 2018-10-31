using System;

namespace Keeper2018
{
    [Serializable]
    public class SerializableAccount
    {
        public int Id;
        public int OwnerId;
        public string Header;
        public bool IsExpanded;
    }
}