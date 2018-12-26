using System.Collections.Generic;

namespace Keeper2018
{
    public class ComboTreesProvider
    {
        private readonly KeeperDb _db;

        public List<AccName> MyAccNamesForIncome { get; set; }
        public List<AccName> AccNamesForIncomeTags { get; set; }

        public List<AccName> MyAccNamesForExpense { get; set; }
        public List<AccName> AccNamesForExpenseTags { get; set; }

        public List<AccName> MyAccNamesForTransfer { get; set; }
        public List<AccName> AccNamesForTransferTags { get; set; }

        public List<AccName> MyAccNamesForExchange { get; set; }
        public List<AccName> AccNamesForExchangeTags { get; set; }

        public List<AccName> AccNamesForFilterTags { get; set; }

        public ComboTreesProvider(KeeperDb db)
        {
            _db = db;
        }

        public void Initialize()
        {
            InitializeListsForIncome();
            InitializeListsForExpense();
            InitializeListsForTransfer();
            InitializeListsForExchange();
            InitializeListForFilterTags();
        }

        private void InitializeListForFilterTags()
        {
            // All Tags
            AccNamesForFilterTags = new List<AccName>();
            var list = new List<string>() { "Внешние", "Все доходы", "Все расходы" };
            foreach (var element in list)
            {
                var root = new AccName().PopulateFromAccount(_db.SeekAccount(element), null);
                AccNamesForFilterTags.Add(root);
            }
        }
        private void InitializeListsForIncome()
        {
            // Income
            MyAccNamesForIncome = new List<AccName>
            {
                new AccName().PopulateFromAccount(_db.SeekAccount("Мои"),
                    new List<string> {"Закрытые", "Закрытые депозиты", "Мне должны"})
            };

            // Income Tags
            AccNamesForIncomeTags = new List<AccName>();
            var list = new List<string>() { "ДеньгоДатели", "Банки", "Государство", "Все доходы" };
            foreach (var element in list)
            {
                var root = new AccName().PopulateFromAccount(_db.SeekAccount(element), null);
                AccNamesForIncomeTags.Add(root);
            }
        }

        private void InitializeListsForExpense()
        {
            // Expense
            MyAccNamesForExpense = new List<AccName>
            {
                new AccName().PopulateFromAccount(_db.SeekAccount("Мои"),
                    new List<string> {"Закрытые", "Закрытые депозиты", "Мне должны", "Депозиты"})
            };

            // Expense Tags
            AccNamesForExpenseTags = new List<AccName>();
            var list = new List<string>() { "ДеньгоПолучатели", "Банки", "Государство", "Все расходы" };
            foreach (var element in list)
            {
                var root = new AccName().PopulateFromAccount(_db.SeekAccount(element), null);
                AccNamesForExpenseTags.Add(root);
            }
        }

        private void InitializeListsForTransfer()
        {
            // Transfer
            MyAccNamesForTransfer = new List<AccName>
            {
                new AccName().PopulateFromAccount(_db.SeekAccount("Мои"),
                                                       new List<string> {"Закрытые", "Закрытые депозиты"})
            };

            // Transfer Tags
            AccNamesForTransferTags = new List<AccName>();
            var list = new List<string>() { "Форекс" };
            foreach (var element in list)
            {
                var root = new AccName().PopulateFromAccount(_db.SeekAccount(element), null);
                AccNamesForTransferTags.Add(root);
            }
        }

        private void InitializeListsForExchange()
        {
            // Exchange
            MyAccNamesForExchange = new List<AccName>
            {
                new AccName().PopulateFromAccount(_db.SeekAccount("Мои"),
                                                       new List<string> {"Закрытые", "Закрытые депозиты"})
            };

            // Exchange Tags
            AccNamesForExchangeTags = new List<AccName>();
            var list = new List<string>() { "Банки", "Форекс" };
            foreach (var element in list)
            {
                var root = new AccName().PopulateFromAccount(_db.SeekAccount(element), null);
                AccNamesForExchangeTags.Add(root);
            }
        }

    }
}