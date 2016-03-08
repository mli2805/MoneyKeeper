using System.Collections.Generic;
using Keeper.DomainModel;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.AccountEditing;

namespace Keeper.ViewModels.TransWithTags
{
    public static class ListsForComboTrees
    {
        public static readonly AccountTreeStraightener AccountTreeStraightener = new AccountTreeStraightener();
        public static List<AccName> MyAccNamesForIncome { get; set; } = new List<AccName>();
        public static List<AccName> MyAccNamesForExpense { get; set; } = new List<AccName>();
        public static List<AccName> AccNamesForIncomeTags { get; set; } = new List<AccName>();

        public static void InitializeLists(KeeperDb db)
        {
            MyAccNamesForIncome.Add(new AccName().PopulateFromAccount(AccountTreeStraightener.Seek("Мои", db.Accounts)));

            var list = new List<string>() { "ДеньгоДатели", "Банки", "Государство", "Все доходы" };
            foreach (var element in list)
            {
                var root = new AccName().PopulateFromAccount(AccountTreeStraightener.Seek(element, db.Accounts));
                AccNamesForIncomeTags.Add(root);
            }

            MyAccNamesForExpense.Add(new AccName().PopulateFromAccount(AccountTreeStraightener.Seek("Мои", db.Accounts)));
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