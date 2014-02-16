using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Caliburn.Micro;
using Keeper.Models;
using Keeper.Models.Shell;

namespace Keeper.DomainModel
{
	[Serializable]
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
		public ObservableCollection<Account> Children { get; private set; }
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

		public string ToDepositFormat()
		{
			// BUG: Dirty hack 
			var shiftedName = new string(' ', 4) + Name;
			return Id + " ; " + shiftedName + " ; " + (Parent == null ? 0 : Parent.Id) + " ; " + IsExpanded;
		}

		public override string ToString()
		{
			return Name;
		}

		/// <summary>
		/// true ���� ������� ��� ������� �����-��������� ��� ��� ���� ����
		/// </summary>
		/// <param name="ancestor"></param>
		/// <returns></returns>
		public bool Is(string ancestor)  // Descendant - ������� ; Ancestor - ������
		{
			if (Name == ancestor) return true;
			return Parent != null && Parent.Is(ancestor);
		}

		public bool Is(Account ancestor)
		{
			if (this == ancestor) return true;
			return Parent != null && Parent.Is(ancestor);
		}

		public int CompareTo(object obj)
		{
			return String.Compare(Name, ((Account)obj).Name, StringComparison.Ordinal);
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
				result = Convert.ToDateTime(s.Substring(p - 2, n - p + 2), new CultureInfo("ru-RU"));
			}
			catch (Exception)
			{

				result = new DateTime(0);
			}
			return result;
		}

		public static int CompareEndDepositDates(Account a, Account b)
		{
			// �������� ������� �� ��� �� �������������� �����
			// �� ���� ��� ����� �� ������������� ������ ��� �������� 
			// �� � �������� ���� ����� ������ ����� ��������� 1/01/0001
			return DateTime.Compare(GetEndDepositDate(a.Name), GetEndDepositDate(b.Name));
		}

	}
}