﻿using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class AccName : PropertyChangedBase, ITreeViewItemModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public AccName Parent { get; set; }
        public List<AccName> Children { get; private set; } = new List<AccName>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account">корневой account, с которого начать</param>
        /// <param name="cutBranches">список ветвей, которые обрубить</param>
        /// <returns></returns>
        public AccName PopulateFromAccount(AccountModel account, List<string> cutBranches)
        {
            var result = new AccName();
            result.Id = account.Id;
            result.Name = account.Name;

            foreach (var child in account.Children)
            {
                if (cutBranches?.FirstOrDefault(st => st == child.Name) != null) continue;
                var resultChild =  PopulateFromAccount(child, cutBranches);
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