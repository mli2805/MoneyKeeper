using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Composition;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.Utils.AccountEditing;
using Keeper.Utils.CommonKeeper;

namespace Keeper.ViewModels.SingleViews
{
    [Export]
    class ArticlesAssociationsViewModel : Screen
    {
        private static KeeperDb _db;
        private readonly ComboboxCaterer _comboboxCaterer;

        public ObservableCollection<ArticleAssociation> Rows { get; set; }
        public static List<Account> ExternalAccounts { get; private set; }

        public static List<Account> AssociatedArticles { get; private set; }

        public static List<OperationType> OperationTypes { get; private set; }

        [ImportingConstructor]
        public ArticlesAssociationsViewModel(KeeperDb db, ComboboxCaterer comboboxCaterer)
        {
            _db = db;
            _comboboxCaterer = comboboxCaterer;

            InitializeListsForCombobox();

            Rows = _db.ArticlesAssociations;

            var view = CollectionViewSource.GetDefaultView(Rows);
            view.SortDescriptions.Add(new SortDescription("OperationType", ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription("ExternalAccount", ListSortDirection.Ascending));
        }

        private void InitializeListsForCombobox()
        {
            ExternalAccounts = _comboboxCaterer.GetExternalAccounts();
            AssociatedArticles =_comboboxCaterer.GetIncomeAndExpenseArticles();
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
