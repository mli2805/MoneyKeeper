using System;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using Keeper.ViewModels;

namespace Keeper.DomainModel
{
  [Serializable]
  public class Account : PropertyChangedBase, IComparable
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
    public ObservableCollection<Account> Children { get; private set; }

    #region ' _isSelected '
    [NonSerialized]
    private bool _isSelected;
    public bool IsSelected
    {
      get { return _isSelected; }
      set
      {
        if (value.Equals(_isSelected)) return;
        _isSelected = value;
        if (_isSelected) IoC.Get<ShellViewModel>().SelectedAccount = this;
      }
    }
    #endregion

    public bool IsExpanded { get; set; }
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

    #region Override == , != , Equals and GetHashCode

    public static bool operator ==(Account a, Account b)
    {
      // If both are null, or both are same instance, return true.
      if (ReferenceEquals(a, b)) return true;
      // If one is null, but not both, return false.
      if (((object)a == null) || ((object)b == null)) return false;
      return a.Name == b.Name;
    }

    public static bool operator !=(Account a,Account b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      if (object.ReferenceEquals(this, obj)) return true;
      var other = obj as Account;
      if (other == null) return false;
      return this.Name == other.Name;
    }

    public override int GetHashCode()
    {
      return Name.GetHashCode();
    }

    #endregion

    public static void CopyForEdit(Account destination, Account source)
    {
      destination.Id = source.Id;
      destination.Name = source.Name;
      destination.Parent = source.Parent;

      foreach (var account in source.Children)
      {
        var child = new Account();
        CopyForEdit(child,account);
        destination.Children.Add(child);
      }
    }

    private int ParentForDump()
    {
      return Parent == null ? 0 : Parent.Id;
    }

    public string ToDump(int offset)
    {

      string shiftedName = Name.PadLeft(Name.Length + offset);
      return Id + " ; " + shiftedName + " ; " + ParentForDump() + " ; " + IsExpanded;
    }

    public override string ToString()
    {
      return Name;
    }

    public string GetRootName()
    {
      return Parent == null ? Name : Parent.GetRootName();
    }


    /// <summary>
    /// true если инстанс потомок счета-параметра, но не сам этот счет
    /// </summary>
    /// <param name="ancestor"></param>
    /// <returns></returns>
    public bool IsDescendantOf(string ancestor)  // Descendant - потомок ; Ancestor - предок
    {
      if (Parent == null) return false;
      return Parent.Name == ancestor || Parent.IsDescendantOf(ancestor);
    }

    public bool IsDescendantOf(Account ancestor)
    {
      if (Parent == null) return false;
      return Parent == ancestor || Parent.IsDescendantOf(ancestor);
    }

    /// <summary>
    /// true если инстанс или потомок счета-параметра или сам ЭТОТ счет
    /// </summary>
    /// <param name="ancestor"></param>
    /// <returns></returns>
    public bool IsTheSameOrDescendantOf(string ancestor)  // Descendant - потомок ; Ancestor - предок
    {
      if (Name == ancestor) return true;
      return Parent == null ? false : Parent.IsTheSameOrDescendantOf(ancestor);
    }

    public bool IsTheSameOrDescendantOf(Account ancestor)  
    {
      if (this == ancestor) return true;
      return Parent == null ? false : Parent.IsTheSameOrDescendantOf(ancestor);
    }



    public int CompareTo(object obj)
    {
      return String.Compare(Name, ((Account) obj).Name, StringComparison.Ordinal);
    }

    public static DateTime GetEndDepositDate(string depositName)
    {
      var s = depositName;
      var p = s.IndexOf('/'); if (p == -1) return new DateTime(0);
      var n = s.IndexOf(' ', p); if (n == -1) return new DateTime(0);
      p = s.IndexOf('/', n); if (p == -1) return new DateTime(0);
      n = s.IndexOf(' ', p); if (n == -1) return new DateTime(0);

      DateTime result;
      try
      {
        result = Convert.ToDateTime(s.Substring(p - 2, n - p + 2));
      }
      catch (Exception)
      {
        
        result = new DateTime(0);
      }
      return result;
    }

    public int CompareOnEndDepositDateBasis(Account other)
    {
      if (Parent == null || Parent.Name != "Депозиты") return 0;
      if (other == null || other.Parent.Name != "Депозиты") return 0;

      return DateTime.Compare(GetEndDepositDate(Name), GetEndDepositDate(other.Name));
    }

    public static int CompareEndDepositDates(Account a, Account b)
    {
      // проверка депозит ли это не осуществляется здесь
      // но если имя счета не соответствует шабону для депозита 
      // то в качестве даты конца вклада будет возвращен 1/01/0001
      return DateTime.Compare(GetEndDepositDate(a.Name), GetEndDepositDate(b.Name));
    }

  }
}