using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public static class TraExtension
    {
        public static int GetTransactionExpenseCategory(this TransactionModel tr, KeeperDataModel dataModel, List<int> expenseGroupsIds)
        {
            foreach (var tag in tr.Tags)
            {
                var id = tag.Id;
                while (true)
                {
                    if (id == 189) break;
                    if (expenseGroupsIds.Contains(id)) return id;
                    var cat = dataModel.AcMoDict[id];
                    if (cat.Owner == null) break;
                    id = cat.Owner.Id;
                }
            }
            // never comes here, every expense transaction should have expense category
            return -1;
        }

        public static int GetTransactionExpenseCategory(this TransactionModel tr, KeeperDataModel dataModel)
        {
            var expenseGroupsIds = dataModel.AcMoDict[189].Children.Select(c=>c.Id).ToList();
            return tr.GetTransactionExpenseCategory(dataModel, expenseGroupsIds);
        }

        public static string GetCounterpartyName(this TransactionModel tr, KeeperDataModel dataModel)
        {
                                                                                    // внешние
            foreach (var trTag in tr.Tags.Where(trTag => trTag.Is(157)))
            {
                return trTag.Name;
            }

            return "контрагент не найден";
        }

    }
}