﻿using System;
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

namespace Keeper.ViewModels.SingleViews
{
    [Export]
    class ArticlesAssociationsViewModel : Screen
    {
        private readonly KeeperDb _db;
        private readonly AccountTreeStraightener _accountTreeStraightener;

        public ObservableCollection<ArticleAssociation> Rows { get; set; }
        public static List<Account> ExternalAccounts { get; private set; }
        public static List<Account> AssociatedArticles { get; private set; }

        public static List<OperationType> OperationTypes { get; private set; }

        [ImportingConstructor]
        public ArticlesAssociationsViewModel(KeeperDb db, AccountTreeStraightener accountTreeStraightener)
        {
            _db = db;
            _accountTreeStraightener = accountTreeStraightener;

            InitializeListsForCombobox();

            Rows = _db.ArticlesAssociations;

            var view = CollectionViewSource.GetDefaultView(Rows);
            view.SortDescriptions.Add(new SortDescription("OperationType", ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription("ExternalAccount", ListSortDirection.Ascending));
        }

        private void InitializeListsForCombobox()
        {
            ExternalAccounts =
              (_accountTreeStraightener.Flatten(_db.Accounts).Where(
                account => account.Is("Внешние") && account.Children.Count == 0)).ToList();
            ExternalAccounts.Add(_accountTreeStraightener.Seek("Банки",_db.Accounts));

            AssociatedArticles = (_accountTreeStraightener.Flatten(_db.Accounts).
               Where(account => (account.Is("Все доходы") || account.Is("Все расходы")) && account.Children.Count == 0)).ToList();

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
