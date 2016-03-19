using System.Collections.Generic;
using Caliburn.Micro;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.AccountEditing;

namespace Keeper.ViewModels.TransWithTags
{
    public static class ListsForComboTrees
    {
        public static readonly AccountTreeStraightener AccountTreeStraightener = new AccountTreeStraightener();
        public static List<AccName> MyAccNamesForIncome { get; set; }
        public static List<AccName> MyAccNamesForExpense { get; set; }
        public static List<AccName> AccNamesForIncomeTags { get; set; }
        public static List<AccName> AccNamesForExpenseTags { get; set; }

        public static void InitializeListsForIncome(KeeperDb db)
        {
            // Income
            MyAccNamesForIncome = new List<AccName>
            {
                new AccName().PopulateFromAccount(AccountTreeStraightener.Seek("Мои", db.Accounts),
                    new List<string> {"Закрытые", "Закрытые депозиты", "Мне должны"})
            };

            // Income Tags
            AccNamesForIncomeTags = new List<AccName>();
            var list = new List<string>() { "ДеньгоДатели", "Банки", "Государство", "Все доходы" };
            foreach (var element in list)
            {
                var root = new AccName().PopulateFromAccount(AccountTreeStraightener.Seek(element, db.Accounts), null);
                AccNamesForIncomeTags.Add(root);
            }
        }

        public static void InitializeListsForExpense(KeeperDb db)
        {
            // Expense
            MyAccNamesForExpense = new List<AccName>
            {
                new AccName().PopulateFromAccount(AccountTreeStraightener.Seek("Мои", db.Accounts),
                    new List<string> {"Закрытые", "Закрытые депозиты", "Мне должны", "Депозиты"})
            };

            // Expense Tags
            AccNamesForExpenseTags = new List<AccName>();
            var list = new List<string>() { "ДеньгоПолучатели", "Банки", "Государство", "Все расходы" };
            foreach (var element in list)
            {
                var root = new AccName().PopulateFromAccount(AccountTreeStraightener.Seek(element, db.Accounts), null);
                AccNamesForExpenseTags.Add(root);
            }
        }

        public static AccName FindThroughTheForest(this List<AccName> roots, string name)
        {
            foreach (var root in roots)
            {
                var result = root.FindThroughTree(name);
                if (result != null) return result;
            }
            return null;
        }

    }
}