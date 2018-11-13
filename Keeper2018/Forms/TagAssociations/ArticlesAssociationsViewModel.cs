using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class ArticlesAssociationsViewModel : Screen
    {
        private static KeeperDb _db;

        public ObservableCollection<TagAssociationModel> Rows { get; set; }
        public static List<AccountModel> ExternalAccounts { get; private set; }

        public static List<AccountModel> AssociatedArticles { get; private set; }

        public static List<OperationType> OperationTypes { get; private set; }
        public static List<AssociationType> AssociationTypes { get; private set; }

        public ArticlesAssociationsViewModel(KeeperDb db)
        {
            _db = db;

            InitializeListsForCombobox();


//            var view = CollectionViewSource.GetDefaultView(Rows);
//            view.SortDescriptions.Add(new SortDescription("OperationType", ListSortDirection.Ascending));
//            view.SortDescriptions.Add(new SortDescription("ExternalAccount", ListSortDirection.Ascending));
        }

        private void InitializeListsForCombobox()
        {
            ExternalAccounts = _db.GetLeavesOf("Внешние");
            AssociatedArticles = _db.GetLeavesOf("Все доходы");
            AssociatedArticles.AddRange(_db.GetLeavesOf("Все расходы"));
            OperationTypes = Enum.GetValues(typeof (OperationType)).Cast<OperationType>().ToList();
            AssociationTypes = Enum.GetValues(typeof (AssociationType)).Cast<AssociationType>().ToList();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Ассоциации категорий";

            Rows = _db.AssociationModels;
        }

        public void CloseView()
        {
            TryClose();
        }

    }
}
