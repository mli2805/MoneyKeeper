using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class AccName : PropertyChangedBase, ITreeViewItemModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ButtonName { get; set; }
        public AccName Parent { get; set; }
        public List<AccName> Children { get; private set; } = new List<AccName>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account">корневой account, с которого начать</param>
        /// <param name="cutBranches">список ветвей, которые обрубить</param>
        /// <returns></returns>
        public AccName PopulateFromAccount(AccountItemModel account, List<int> cutBranches)
        {
            var result = new AccName { Id = account.Id, Name = account.Name, ButtonName = account.ButtonName };

            foreach (var child in account.Children)
            {
                if (cutBranches != null && cutBranches.Any(b=>b == child.Id)) continue;
                var resultChild =  PopulateFromAccount((AccountItemModel)child, cutBranches);
                resultChild.Parent = result;
                result.Children.Add(resultChild);
            }
            return result;
        }

        public override string ToString()
        {
            return Name;
        }

        #region ITreeViewItemModel members
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
        #endregion
    }
}