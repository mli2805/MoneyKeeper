using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Keeper2018
{
    public class AccountModel : TreeViewItem, IComparable
    {
        public int Id { get; set; }

        public AccountModel Owner { get; set; } // property Parent is occupied in TreeViewItem

                                           // Items are in TreeViewItem
        public List<AccountModel> Children => Items.Cast<AccountModel>().ToList();
        public Deposit Deposit { get; set; }

        public new string Name => (string) Header;
        public bool IsDeposit => Deposit != null;

        public override string ToString() => (string)Header;
        public int CompareTo(object obj)
        {
            return string.Compare(Name, ((AccountModel) obj).Name, StringComparison.Ordinal);
        }

        public AccountModel(string headerText)
        {
            Header = headerText;
            IsExpanded = true;
        }

        public bool IsFolder { get; set; }
      
    }
}