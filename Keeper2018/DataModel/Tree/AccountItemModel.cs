namespace Keeper2018
{
    public class AccountItemModel : TreeViewItemModel
    {
        public string Img { get; set; }

        public AccountItemModel(int id, string name, TreeViewItemModel parent) : base(id, name, parent)
        {
        }

        public void AddChild(TreeViewItemModel child)
        {
            base.Children.Add(child);
        }
    }
}