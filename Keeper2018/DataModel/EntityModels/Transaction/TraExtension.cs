using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class TraExtension
    {
        public static int GetTransactionExpenseCategory(this Transaction tr, KeeperDataModel dataModel, List<int> expenseGroupsIds)
        {
            foreach (var tagId in tr.Tags)
            {
                var id = tagId;
                while (true)
                {
                    if (id == 189) break;
                    if (expenseGroupsIds.Contains(id)) return id;
                    var tag = dataModel.AcMoDict[id];
                    if (tag.Owner == null) break;
                    id = tag.Owner.Id;
                }
            }
            // never comes here, every expense transaction should have expense category
            return -1;
        }

        public static int GetTransactionExpenseCategory(this Transaction tr, KeeperDataModel dataModel)
        {
            var expenseGroupsIds = dataModel.AcMoDict[189].Children.Select(c=>c.Id).ToList();
            return tr.GetTransactionExpenseCategory(dataModel, expenseGroupsIds);
        }

        public static string GetCounterpartyName(this Transaction tr, KeeperDataModel dataModel)
        {
            foreach (var trTag in tr.Tags)
            {
                var tag = dataModel.AcMoDict[trTag];
                if (tag.Is(157)) // внешние
                    return tag.Name;
            }
            return "контрагент не найден";
        }

    }
}