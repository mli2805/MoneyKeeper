using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace Keeper2018
{
    public class TreeViewItemModel : PropertyChangedBase
    {
        #region Data

        public int Id;
        readonly ObservableCollection<TreeViewItemModel> _children;

        bool _isExpanded;
        private bool _isSelected;
        private string _name;
        private TreeViewItemModel _parent;

        #endregion // Data

        #region Constructors

        protected TreeViewItemModel(string name, TreeViewItemModel parent)
        {
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
    }
}
