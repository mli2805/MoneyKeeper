using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using Caliburn.Micro;
using Keeper.ViewModels;

namespace Keeper.DomainModel
{
  public class Account : PropertyChangedBase, IComparable
  {

    #region // �������� (properties) ������

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
        // NotifyOfPropertyChange(() => IsSelected);
        if (_isSelected) IoC.Get<ShellViewModel>().SelectedAccount = this;
      }
    }
    #endregion

    public bool IsExpanded { get; set; }
    #endregion

    #region // ������������

    public Account()
    {
      Name = "";
      Parent = null;
      var observableCollection = new ObservableCollection<Account>();
      Children = observableCollection;
      _isSelected = false;
    }

    public Account(string name)
      : this() // �.�. ������� ����������� ��� ����������, � ����� ��������� ���� ���
    {
      Name = name;
    }

    #endregion

    public static void CopyForEdit(Account destination, Account source)
    {
      destination.Id = source.Id;
      destination.Name = source.Name;
      destination.Parent = source.Parent;
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
    /// true ���� ������� ������� �����-���������, �� �� ��� ���� ����
    /// </summary>
    /// <param name="ancestor"></param>
    /// <returns></returns>
    public bool IsDescendantOf(string ancestor)  // Descendant - ������� ; Ancestor - ������
    {
      if (Parent == null) return false;
      return Parent.Name == ancestor || Parent.IsDescendantOf(ancestor);
    }


    /// <summary>
    /// true ���� ������� ��� ������� �����-��������� ��� ��� ���� ����
    /// </summary>
    /// <param name="ancestor"></param>
    /// <returns></returns>
    public bool IsTheSameOrDescendantOf(string ancestor)  // Descendant - ������� ; Ancestor - ������
    {
      if (Name == ancestor) return true;
      return Parent == null ? false : Parent.IsTheSameOrDescendantOf(ancestor);
    }

    public int CompareTo(object obj)
    {
      return System.String.Compare(Name, ((Account) obj).Name, System.StringComparison.Ordinal);
    }
  }
}