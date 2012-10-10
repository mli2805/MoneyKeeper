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
  public class IncomeCategory : Category
  {
    public IncomeCategory()
    {
    }

    public IncomeCategory(string name) : base(name)
    {
    }

    public override Category NewPrototype()
    {
      return new IncomeCategory();
    }
  }

  public class ExpenseCategory : Category
  {
    public ExpenseCategory()
    {
    }

    public ExpenseCategory(string name) : base(name)
    {
    }

    public override Category NewPrototype()
    {
      return new ExpenseCategory();
    }
  }

  public abstract class Category : PropertyChangedBase
  {
    #region // свойства класса , включая IsSelected
    public int Id { get; set; }
    public string Name { get; set; }
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

    protected Category()
    {
      Name = "";
      Parent = null;
      Children = new ObservableCollection<Category>();
      _isSelected = false;
    }

    protected Category(string name)
      : this()
    {
      Name = name;
    }

    public abstract Category NewPrototype();
  }

}
