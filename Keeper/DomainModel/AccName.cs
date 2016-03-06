using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Keeper.Controls.ComboboxTreeview;

namespace Keeper.DomainModel
{
    public class AccName : PropertyChangedBase, ITreeViewItemModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public AccName Parent { get; set; }
        public List<AccName> Children { get; private set; } = new List<AccName>();

        public AccName PopulateFromAccount(Account account)
        {
            var result = new AccName();
            result.Id = account.Id;
            result.Name = account.Name;

            foreach (var child in account.Children)
            {
                var resultChild = PopulateFromAccount(child);
                resultChild.Parent = result;
                result.Children.Add(resultChild);
            }
            return result;
        }

        public AccName FindThroughTree(string name)
        {
            if (name == Name) return this;
            foreach (var child in Children)
            {
                var result = child.FindThroughTree(name);
                if (result != null) return result;
            }
            return null;
        }

        public string SelectedValuePath => Name;
        public string DisplayValuePath => Name;
        public bool IsExpanded { get; set; }
        public bool IsSelected { get; set; }
        private IEnumerable<AccName> GetAscendingHierarchy()
        {
            var accName = this;

            yield return accName;
            while (accName.Parent != null)
            {
                yield return accName.Parent;
                accName = accName.Parent;
            }
        }
        public IEnumerable<ITreeViewItemModel> GetHierarchy()
        {
            return GetAscendingHierarchy().Reverse();
        }
        public IEnumerable<ITreeViewItemModel> GetChildren()
        {
            return Children;
        }
    }
}