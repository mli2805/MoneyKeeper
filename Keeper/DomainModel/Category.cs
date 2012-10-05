using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Keeper.DomainModel
{
  public class Category : PropertyChangedBase
  {
    public string Name { get; set; }
    public Category Parent { get; set; }
    public List<Category> Children { get; set; }

    #region ' _isSelected '

    private bool _isSelected;

    public bool IsSelected
    {
      get { return _isSelected; }
      set
      {
        if (value.Equals(_isSelected)) return;
        _isSelected = value;
        NotifyOfPropertyChange(() => IsSelected);
      }
    }

    #endregion

    public Category()
    {
      Name = "";
      Parent = null;
      Children = new List<Category>();
      _isSelected = false;
    }

    public Category(string name) : this()
    {
      Name = name;
    }

  }

}
