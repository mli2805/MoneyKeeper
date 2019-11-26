using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public static class TraExtension
    {
        public static int GetTransactionExpenseCategory(this Transaction tr, KeeperDb db, List<int> expenseGroupsIds)
        {
            foreach (var tagId in tr.Tags)
            {
                var id = tagId;
                while (true)
                {
                    if (id == 189) break;
                    if (expenseGroupsIds.Contains(id)) return id;
                    var tag = db.AcMoDict[id];
                    if (tag.Owner == null) break;
                    id = tag.Owner.Id;
                }
            }
            // never comes here, every expense transaction should have expense category
            return -1;
        }

        public static int GetTransactionExpenseCategory(this Transaction tr, KeeperDb db)
        {
            var expenseGroupsIds = db.AcMoDict[189].Children.Select(c=>c.Id).ToList();
            return tr.GetTransactionExpenseCategory(db, expenseGroupsIds);
        }

        public static string GetCounterpartyName(this Transaction tr, KeeperDb db)
        {
            foreach (var trTag in tr.Tags)
            {
                var tag = db.AcMoDict[trTag];
                if (tag.Is(157)) // внешние
                    return tag.Name;
            }
            return "контрагент не найден";
        }

    }
}