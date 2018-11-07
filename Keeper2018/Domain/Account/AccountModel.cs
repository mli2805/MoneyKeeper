using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Keeper2018
{
    public class AccountModel : TreeViewItem
    {
        public int Id { get; set; }

        public AccountModel Owner { get; set; } // property Parent is occupied in TreeViewItem

                                           // Items are in TreeViewItem
        public List<AccountModel> Children => Items.Cast<AccountModel>().ToList();

        public new string Name => (string) Header;

        public override string ToString() => (string)Header;

        public AccountModel(string headerText)
        {
            Header = headerText;
            IsExpanded = true;
        }


        public bool Is(AccountModel ancestor)
        {
            if (Equals(this, ancestor)) return true;
            return Owner != null && Owner.Is(ancestor);
        }
    }
}