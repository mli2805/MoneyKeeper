using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace Keeper2018
{
    public class TreeViewItemModel : PropertyChangedBase
    {
        #region Data

        public int Id { get; set; }
        readonly ObservableCollection<TreeViewItemModel> _children;

        bool _isExpanded;
        private bool _isSelected;
        private string _name;
        private TreeViewItemModel _parent;

        #endregion // Data

        #region Constructors

        protected TreeViewItemModel(int id, string name, TreeViewItemModel parent)
        {
            Id = id;
            _name = name;
            _parent = parent;

            _children = new ObservableCollection<TreeViewItemModel>();
        }

        #endregion // Constructors

        #region Presentation Members

        public string Name  
        {
            get => _name;
            set
            {
                if (value == _name) return;
                _name = value;
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<TreeViewItemModel> Children { get { return _children; } }


        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (value == _isExpanded) return;
                _isExpanded = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (value == _isSelected) return;
                _isSelected = value;
                NotifyOfPropertyChange();
            }
        }

        public TreeViewItemModel Parent
        {
            get => _parent;
            set => _parent = value;
        }


        #endregion

        public bool Is(TreeViewItemModel treeViewItemModel)
        {
            if (Equals(treeViewItemModel)) return true;
            return Parent != null && Parent.Is(treeViewItemModel);
        }

        public bool Is(int accountId)
        {
            if (accountId == Id) return true;
            return Parent != null && Parent.Is(accountId);
        }

        public TreeViewItemModel IsC(TreeViewItemModel treeViewItemModel)
        {
            if (Equals(treeViewItemModel)) return this;
            if (Parent == null) return null;
            if (Parent.Equals(treeViewItemModel)) return this;
            return Parent.IsC(treeViewItemModel);
        }

    }
}
