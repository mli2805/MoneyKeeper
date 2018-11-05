using System;

namespace Keeper2018
{
    [Serializable]
    public class Account
    {
        public int Id;
        public int OwnerId;
        public string Header;
        public bool IsExpanded;
        public string Name => Header;
    }
}