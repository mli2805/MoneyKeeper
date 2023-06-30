using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class TraExtension
    {
        public static int GetTransactionBaseCategory(this TransactionModel tr, 
            KeeperDataModel dataModel, List<int> tagGroupsIds)
        {
            foreach (var tag in tr.Tags)
            {
                var id = tag.Id;
                while (true)
                {
                    if (id == 185 || id == 189) break;
                    if (tagGroupsIds.Contains(id)) return id;
                    var cat = dataModel.AcMoDict[id];
                    if (cat.Parent == null) break;
                    id = cat.Parent.Id;
                }
            }
            // never comes here, every expense transaction should have expense category
            return -1;
        }

        public static int GetTransactionBaseCategory(this TransactionModel tr, KeeperDataModel dataModel, OperationType operationType)
        {
            var expenseGroupsIds = 
                dataModel.AcMoDict[operationType == OperationType.Доход ? 185 :189]
                .Children.Select(c=>c.Id)
                .ToList();
            return tr.GetTransactionBaseCategory(dataModel, expenseGroupsIds);
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