using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class TagAssociationsViewModel : Screen
    {
        public KeeperDb KeeperDb { get; }

        public static List<Account> ExternalAccounts { get; private set; }
        public static List<Account> AssociatedArticles { get; private set; }
        public static List<OperationType> OperationTypes { get; private set; }
        public static List<AssociationType> Destinations { get; private set; }

        public TagAssociationsViewModel(KeeperDb keeperDb)
        {
            KeeperDb = keeperDb;
            InitializeListsForCombobox();
        }

        private void InitializeListsForCombobox()
        {
//            ExternalAccounts = _comboboxCaterer.GetExternalAccounts();
            ExternalAccounts = new List<Account>();
//            AssociatedArticles =_comboboxCaterer.GetIncomeAndExpenseArticles();
            AssociatedArticles = new List<Account>();
            OperationTypes = Enum.GetValues(typeof (OperationType)).Cast<OperationType>().ToList();
            Destinations = Enum.GetValues(typeof (AssociationType)).Cast<AssociationType>().ToList();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Tag associations";
        }
    }
}
