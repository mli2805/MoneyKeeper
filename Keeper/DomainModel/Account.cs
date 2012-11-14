using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations.Schema;
using Caliburn.Micro;
using Keeper.ViewModels;

namespace Keeper.DomainModel
{
  public class Account : PropertyChangedBase 
  {

    #region // свойства (properties) класса

    public int Id { get; set; }
    private string _name;
    public string Name
    {
      get { return _name; }
      set
      {
        if (value == _name) return;
        _name = value;
        NotifyOfPropertyChange(() => Name);
      }
    }

    public Account Parent { get; set; }
    public virtual ICollection<Account> Children { get; private set; }

    #region ' _isSelected '
    private bool _isSelected;
    [NotMapped]
    public bool IsSelected
    {
      get { return _isSelected; }
      set
      {
        if (value.Equals(_isSelected)) return;
        _isSelected = value;
//        NotifyOfPropertyChange(() => IsSelected);
        if (_isSelected)
        IoC.Get<ShellViewModel>().SelectedAccount = this;
      }
    }
    #endregion

    #endregion

    #region // конструкторы

    public Account()
    {
      Name = "";
      Parent = null;
      var observableCollection = new ObservableCollection<Account>();
      Children = observableCollection;
      _isSelected = false;
    }

    public Account(string name)
      : this() // т.е. вызвать конструктор без параметров, а затем исполнить свой код
    {
      Name = name;
    }

    #endregion

    public static void CopyForEdit(Account destination, Account source)
    {
      destination.Name = source.Name;
      destination.Parent = source.Parent;
    }

    private int ParentForDump()
    {
      if (Parent == null) return 0;
      else return Parent.Id;
    }

    public string ToDump(int offset)
    {

      string shiftedName = Name.PadLeft(Name.Length+offset);
      return Id + " ; " + shiftedName + " ; " + ParentForDump();
    }

    public override string ToString()
    {
      return Name;
    }

    public string GetRootName()
    {
      return Parent == null ? Name : Parent.GetRootName();
    }

    public bool IsDescendantOf(string ancestor)  // Descendant - потомок ; Ancestor - предок
    {
      if (Parent == null) return false;
      else return Parent.Name == ancestor ? true : Parent.IsDescendantOf(ancestor);
    }
  }
}