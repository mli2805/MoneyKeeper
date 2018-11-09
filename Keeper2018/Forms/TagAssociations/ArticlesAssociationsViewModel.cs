using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;

namespace Keeper2018
{
    public class ArticlesAssociationsViewModel : Screen
    {
        private static KeeperDb _db;

        public ObservableCollection<TagAssociation> Rows { get; set; } = new ObservableCollection<TagAssociation>();
        public static List<Account> ExternalAccounts { get; private set; }

        public static List<Account> AssociatedArticles { get; private set; }

        public static List<OperationType> OperationTypes { get; private set; }

        public ArticlesAssociationsViewModel(KeeperDb db)
        {
            _db = db;

            InitializeListsForCombobox();

            _db.TagAssociations.ForEach(t=>Rows.Add(t));

//            var view = CollectionViewSource.GetDefaultView(Rows);
//            view.SortDescriptions.Add(new SortDescription("OperationType", ListSortDirection.Ascending));
//            view.SortDescriptions.Add(new SortDescription("ExternalAccount", ListSortDirection.Ascending));
        }

        private void InitializeListsForCombobox()
        {
            ExternalAccounts = _db.GetAccountsOf("Внешние");
            AssociatedArticles = _db.GetAccountsOf("Все доходы");;
            AssociatedArticles.AddRange(_db.GetAccountsOf("Все расходы"));
            OperationTypes = Enum.GetValues(typeof (OperationType)).Cast<OperationType>().ToList();
        }

        protected override void OnViewLoaded(object view)
        {
            InitializeListsForCombobox();
            DisplayName = "Ассоциации категорий";
        }

        public void CloseView()
        {
            TryClose();
        }

    }
}
