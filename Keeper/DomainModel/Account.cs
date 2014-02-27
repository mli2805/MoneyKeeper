using System;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using Keeper.Models.Shell;

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

    /* IsActive == false не только закрытые депозиты и счета(карточки) 
     * но и например контрагенты, с которыми больше не сотрудничаем 
     * (например, предыдущие работадатели)
     * статьи более не используемые (типа иррациональные)
     */
    public bool IsActive { get; set; }

    public Deposit Deposit { get; set; }

		#region ' _isSelected '
		[NonSerialized]
		private bool _isSelected;
		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
//				if (value.Equals(_isSelected)) return;
				_isSelected = value;
				NotifyOfPropertyChange();
        if (_isSelected) IoC.Get<ShellModel>().MyForestModel.SelectedAccount = this;
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

		public static bool operator !=(Account a, Account b)
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
		  destination.IsActive = source.IsActive;
			destination.Parent = source.Parent;

      if (source.Deposit != null)
      {
        destination.Deposit = (Deposit) source.Deposit.Clone();
        destination.Deposit.ParentAccount = destination;
      }

		  foreach (var account in source.Children)
			{
				var child = new Account();
				CopyForEdit(child, account);
				destination.Children.Add(child);
			}
		}

		public override string ToString()
		{
			return Name;
		}

		/// <summary>
		/// true если инстанс или потомок счета-параметра или сам ЭТОТ счет
		/// </summary>
		/// <param name="ancestor"></param>
		/// <returns></returns>
		public bool Is(string ancestor)  // Descendant - потомок ; Ancestor - предок
		{
			if (Name == ancestor) return true;
			return Parent != null && Parent.Is(ancestor);
		}

		public bool Is(Account ancestor)
		{
			if (this == ancestor) return true;
			return Parent != null && Parent.Is(ancestor);
		}

    public bool IsLeaf(string ancestor)
    {
      return Is(ancestor) && Children.Count == 0;
    }

		public int CompareTo(object obj)
		{
			return String.Compare(Name, ((Account)obj).Name, StringComparison.Ordinal);
		}

    public static int CompareAccountsByDepositFinishDate(Account a, Account b)
    {
      if (a.Deposit == null || b.Deposit == null) return 0;
      return DateTime.Compare(a.Deposit.FinishDate, b.Deposit.FinishDate);
    }

	}
}