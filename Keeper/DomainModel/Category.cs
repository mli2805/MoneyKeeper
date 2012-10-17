using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Keeper.ViewModels;

namespace Keeper.DomainModel
{
  public class Category : PropertyChangedBase
  {
    #region // свойства класса , включая IsSelected
    public int Id { get; set; }
    private string _name;
    public string Name
    {
      get { return _name; }
      set 
      { 
        if (value.Equals(_name)) return;
        _name = value;
        NotifyOfPropertyChange(() => Name);
      }
    }
    public Category Parent { get; set; }
    public virtual ICollection<Category> Children { get; set; }

    #region // _isSelected

    private bool _isSelected;

    public bool IsSelected
    {
      get { return _isSelected; }
      set
      {
        if (value.Equals(_isSelected)) return;
        _isSelected = value;
        NotifyOfPropertyChange(() => IsSelected);
        IoC.Get<ShellViewModel>().SelectedCategory = this;
      }
    }

    #endregion
    #endregion

    public Category()
    {
      Name = "";
      Parent = null;
      Children = new ObservableCollection<Category>();
      _isSelected = false;
    }

    public Category(string name)
      : this()
    {
      Name = name;
    }

    public static void CopyForEdit(Category destination, Category source)
    {
      destination.Name = source.Name;
      destination.Parent = source.Parent;
    }

  }

}
