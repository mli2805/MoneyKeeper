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

    public string Name { get; set; }
    public CurrencyCodes Currency { get; set; }
    public string FullName { get { if (IsAggregate) return Name; else return Name + "  (" + Currency + ")";   } }
    public decimal Balance { get; set; }
    public Account Parent { get; set; }
    public ObservableCollection<Account> Children { get; private set; }
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
      Children = new ObservableCollection<Account>();
      Children.CollectionChanged+=ChildrenOnCollectionChanged;
      _isSelected = false;
      IsAggregate = false;
    }

    public Account(string name)
      : this() // т.е. вызвать конструктор без параметров, а затем исполнить свой код
    {
      Name = name;
    }

    #endregion

  }
}