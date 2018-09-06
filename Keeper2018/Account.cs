using System.Windows.Controls;

namespace Keeper2018
{
    public class Account : TreeViewItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
      //  public Account Parent { get; set; }
      //  public new ObservableCollection<Account> Items { get; set; }
        public int DepositCode { get; set; } = -1;

        public Account()
        {
            IsExpanded = true;
        }

        public Account(string headerText)
        {
            Header = headerText;
            IsExpanded = true;
            
        }
    }
}