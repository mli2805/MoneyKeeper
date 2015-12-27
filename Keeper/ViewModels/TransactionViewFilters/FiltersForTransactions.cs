using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.ViewModels.Transactions;

namespace Keeper.ViewModels.TransactionViewFilters
{
    public class FiltersForTransactions : PropertyChangedBase
    {
        public ICollectionView SortedRows { get; set; }

        public List<AccountFilter> DebetFilterList { get; set; }
        public List<AccountFilter> CreditFilterList { get; set; }
        public List<AccountFilter> ArticleFilterList { get; set; }
        public void InitializeFiltersLists(ListsForComboboxes listsForComboboxes)
        {
            DebetFilterList = BuildAccountFilterList(listsForComboboxes.DebetAccounts);
            CreditFilterList = BuildAccountFilterList(listsForComboboxes.CreditAccounts);
            ArticleFilterList = BuildAccountFilterList(listsForComboboxes.ArticleAccounts);
        }

        public List<AccountFilter> BuildAccountFilterList(List<Account> accounts)
        {
            var filterList = new List<AccountFilter>();

            // <no filter>
            var filter = new AccountFilter();
            filterList.Add(filter);

            foreach (var account in accounts)
            {
                filter = new AccountFilter(account);
                filterList.Add(filter);
            }
            return filterList;
        }

        public FiltersForTransactions(ListsForComboboxes listsForComboboxes, ObservableCollection<Transaction> Rows)
        {
            InitializeFiltersLists(listsForComboboxes);
            SortedRows = CollectionViewSource.GetDefaultView(Rows);
            SortedRows.SortDescriptions.Add(new SortDescription("Timestamp", ListSortDirection.Ascending));
            ClearAllFilters();
            SortedRows.Filter += OnFilter;
        }

        private OperationTypesFilter _selectedOperationTypeFilter;
        public OperationTypesFilter SelectedOperationTypeFilter
        {
            get { return _selectedOperationTypeFilter; }
            set
            {
                if (Equals(value, _selectedOperationTypeFilter)) return;
                _selectedOperationTypeFilter = value;
                NotifyOfPropertyChange(() => SelectedOperationTypeFilter);
                var view = CollectionViewSource.GetDefaultView(SortedRows);
                view.Refresh();
            }
        }

        private AccountFilter _selectedDebetFilter;
        public AccountFilter SelectedDebetFilter
        {
            get { return _selectedDebetFilter; }
            set
            {
                if (Equals(value, _selectedDebetFilter)) return;
                _selectedDebetFilter = value;
                NotifyOfPropertyChange(() => SelectedDebetFilter);
                var view = CollectionViewSource.GetDefaultView(SortedRows);
                view.Refresh();
            }
        }

        private AccountFilter _selectedCreditFilter;
        public AccountFilter SelectedCreditFilter
        {
            get { return _selectedCreditFilter; }
            set
            {
                if (Equals(value, _selectedCreditFilter)) return;
                _selectedCreditFilter = value;
                NotifyOfPropertyChange(() => SelectedCreditFilter);
                var view = CollectionViewSource.GetDefaultView(SortedRows);
                view.Refresh();
            }
        }

        private AccountFilter _selectedArticleFilter;
        public AccountFilter SelectedArticleFilter
        {
            get { return _selectedArticleFilter; }
            set
            {
                if (Equals(value, _selectedArticleFilter)) return;
                _selectedArticleFilter = value;
                NotifyOfPropertyChange(() => SelectedArticleFilter);
                var view = CollectionViewSource.GetDefaultView(SortedRows);
                view.Refresh();
            }
        }

        private string _commentFilter;
        public string CommentFilter
        {
            get { return _commentFilter; }
            set
            {
                if (value == _commentFilter) return;
                _commentFilter = value;
                NotifyOfPropertyChange(() => CommentFilter);
                var view = CollectionViewSource.GetDefaultView(SortedRows);
                view.Refresh();
            }
        }

        public bool OnFilter(object o)
        {
            var transaction = (Transaction)o;
            if (SelectedOperationTypeFilter.IsOn && transaction.Operation != SelectedOperationTypeFilter.Operation) return false;
            if (SelectedDebetFilter.IsOn && transaction.Debet != SelectedDebetFilter.WantedAccount) return false;
            if (SelectedCreditFilter.IsOn && transaction.Credit != SelectedCreditFilter.WantedAccount) return false;
            if (SelectedArticleFilter.IsOn && transaction.Article != SelectedArticleFilter.WantedAccount) return false;
            if (CommentFilter != "" && transaction.Comment.IndexOf(CommentFilter, StringComparison.Ordinal) == -1) return false;

            return true;
        }
        public void ClearAllFilters()
        {
            SelectedOperationTypeFilter = OperationTypesFilterListForCombo.FilterList.First(f => !f.IsOn);
            SelectedDebetFilter = DebetFilterList.First(f => !f.IsOn);
            SelectedCreditFilter = CreditFilterList.First(f => !f.IsOn);
            SelectedArticleFilter = ArticleFilterList.First(f => !f.IsOn);
            CommentFilter = "";
        }

    }
}
