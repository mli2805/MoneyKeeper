using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Keeper.Controls.ComboboxTreeview;
using Keeper.Models.Shell;

namespace Keeper.DomainModel.DbTypes
{
    [Serializable]
    public class Account : PropertyChangedBase, IComparable, ITreeViewItemModel
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
        public ObservableCollection<Account> Children { get; private set; } = new ObservableCollection<Account>();

        /* IsActive == false не только закрытые депозиты и счета(карточки) 
         * но и например контрагенты, с которыми больше не сотрудничаем 
         * (например, предыдущие работадатели)
         * статьи более не используемые (типа иррациональные)
         */
        public bool IsClosed { get; set; }

        public Deposit.Deposit Deposit { get; set; }

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

        public bool IsFolder { get; set; }
        public bool IsExpanded { get; set; }
        #endregion

        #region // конструкторы
        public Account() { }
        public Account(string name) { Name = name; }
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
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as Account;
            if (other == null) return false;
            return Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        #endregion

        #region implementation of ITreeViewItemModel
        public string SelectedValuePath => Name;
        public string DisplayValuePath => Name;

        private IEnumerable<Account> GetAscendingHierarchy()
        {
            var account = this;

            yield return account;
            while (account.Parent != null)
            {
                yield return account.Parent;
                account = account.Parent;
            }
        }
        public IEnumerable<ITreeViewItemModel> GetHierarchy()
        {
            return GetAscendingHierarchy().Reverse();
        }

        public IEnumerable<ITreeViewItemModel> GetChildren()
        {
            return Children;
        }
        #endregion

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// true если инстанс или потомок счета-параметра или сам Ё“ќ“ счет
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

        public bool IsDeposit()
        {
            return Deposit != null;
        }

        public bool IsMyNonDeposit()
        {
            return Is("ћои") && Deposit == null;
        }

        public int CompareTo(object obj)
        {
            return String.Compare(Name, ((Account)obj).Name, StringComparison.Ordinal);
        }

        /// <summary>
        /// только дл€ депозитов! 
        /// дл€ обычных счетов не примен€ть!
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int CompareAccountsByDepositFinishDate(Account a, Account b)
        {
            if (a.IsFolder && !b.IsFolder) return 1;
            if (!a.IsFolder && b.IsFolder) return -1;

            if (a.IsFolder && b.IsFolder)
            {
                if (a.Name == "«акрытые депозиты") return 1;
                if (b.Name == "«акрытые депозиты") return -1;
                return 0;
            }

            if (a.Deposit == null || b.Deposit == null) return 0;

            return DateTime.Compare(a.Deposit.FinishDate, b.Deposit.FinishDate);
        }

    }
}