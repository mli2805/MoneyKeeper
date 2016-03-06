using System.Collections.Generic;
using Keeper.ByFunctional.AccountEditing;
using Keeper.DomainModel;

namespace Keeper.ViewModels.Transactions
{
    public static class ListsForComboTrees
    {
        public static readonly AccountTreeStraightener AccountTreeStraightener = new AccountTreeStraightener();
        public static List<AccName> MyAccNamesForIncome { get; set; } = new List<AccName>();
        public static List<AccName> AccNamesForIncomeTags { get; set; } = new List<AccName>();

        public static void InitializeLists(KeeperDb db)
        {
            var myAccNames = new AccName().PopulateFromAccount(AccountTreeStraightener.Seek("Мои", db.Accounts));
            MyAccNamesForIncome.Add(myAccNames);

            var list = new List<string>() { "ДеньгоДатели", "Банки", "Государство", "Все доходы" };
            foreach (var element in list)
            {
                var root = new AccName().PopulateFromAccount(AccountTreeStraightener.Seek(element, db.Accounts));
                AccNamesForIncomeTags.Add(root);
            }
        }

        public static AccName FindThroughTheForest(List<AccName> roots, string name)
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