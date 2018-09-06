using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Keeper2018
{
    public class Account : TreeViewItem
    {
        public int Id { get; set; }

        public Account Owner { get; set; } // property Parent is occupied in TreeViewItem

                                           // Items are in TreeViewItem
        public List<Account> Children => Items.Cast<Account>().ToList();

        public Account(string headerText)
        {
            Header = headerText;
            IsExpanded = true;
        }
    }
}