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
        public bool IsFolder;
        public Deposit Deposit;

        public bool IsDeposit => Deposit != null;
        public string Name
        {
            get => Header;
            set => Header = value;
        }

        public override string ToString() => Name;
    }
}