using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Caliburn.Micro;
using Keeper.ViewModels;

namespace Keeper.DomainModel
{
  public class Account : PropertyChangedBase // соответствует "кошельку"
  {

    #region // свойства (properties) класса

    public int Id { get; set; }
    public string Name
    {
      get { return _name; }
      set
      {
        if (value == _name) return;
        _name = value;
        NotifyOfPropertyChange(() => Name);
        NotifyOfPropertyChange(() => FullName);
      }
    }

    public CurrencyCodes Currency { get; set; }
    public string FullName { get { if (IsAggregate) return Name; else return Name + "  (" + Currency + ")";   } }
    public decimal Balance { get; set; }
    public Account Parent { get; set; }
    public virtual ICollection<Account> Children { get; private set; }
    private bool _isAggregate;
    public bool IsAggregate
    {
      get { return _isAggregate; }
      set
      {
        if (value.Equals(_isAggregate)) return;
        _isAggregate = value;
        NotifyOfPropertyChange(() => IsAggregate);
        NotifyOfPropertyChange(() => FullName);
      }
    }

    private void ChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
    {
      IsAggregate |= Children.Count != 0;

    }

    #region ' _isSelected '
    private bool _isSelected;
    private string _name;

    public bool IsSelected
    {
      get { return _isSelected; }
      set
      {
        if (value.Equals(_isSelected)) return;
        _isSelected = value;
        NotifyOfPropertyChange(() => IsSelected);
        IoC.Get<ShellViewModel>().SelectedAccount = this;
      }
    }
    #endregion

    #endregion

    #region // конструкторы

    public Account()
    {
      Name = "";
      Currency = CurrencyCodes.BYR;
      Balance = 0;
      Parent = null;
      var observableCollection = new ObservableCollection<Account>();
      Children = observableCollection;
      observableCollection.CollectionChanged += ChildrenOnCollectionChanged;
      _isSelected = false;
      IsAggregate = false;
    }

    public Account(string name)
      : this() // т.е. вызвать конструктор без параметров, а затем исполнить свой код
    {
      Name = name;
    }

    public Account(string name, bool isAggregate)
      : this() // т.е. вызвать конструктор без параметров, а затем исполнить свой код
    {
      Name = name;
      IsAggregate = isAggregate;
    }

    public Account(string name, CurrencyCodes currency, bool isAggregate)
      : this() // т.е. вызвать конструктор без параметров, а затем исполнить свой код
    {
      Name = name;
      Currency = currency;
      IsAggregate = isAggregate;
    }

    #endregion

   
    public static void CopyForEdit(Account destination, Account source)
    {
      destination.Name = source.Name;
      destination.Currency = source.Currency;
      destination.Parent = source.Parent;
      destination.IsAggregate = source.IsAggregate;
    }

    private int ParentForDump()
    {
      if (Parent == null) return 0;
      else return Parent.Id;
    }

    public string ToDump()
    {
      return Id + " , " + Name + " , " + Currency + " , " + ParentForDump() + " , " + IsAggregate;
    }
  }
}