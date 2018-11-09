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

        public string Name
        {
            get { return Header; }
            set { Header = value; }
        }

        public override string ToString() => Name;
    }
}