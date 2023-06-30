namespace Keeper2018
{
    public class AccountItemModel : TreeViewItemModel
    {
        public string Img { get; set; }

        public AccountItemModel(string name, TreeViewItemModel parent) : base(name, parent)
        {
        }

        public void AddChild(TreeViewItemModel child)
        {
            base.Children.Add(child);
        }
    }
}