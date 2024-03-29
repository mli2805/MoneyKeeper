using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public class TreeBalance
    {
        public Balance Balance { get; set; }
        public decimal TotalInUsd { get; set; } // каждую операцию переводим по курсу на момент совершения, а не в конце
        public List<string> ShortLines { get; set; } // if leaf
    }
    public class CategoryCalculator
    {
        private readonly KeeperDataModel _dataModel;

        public CategoryCalculator(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Evaluate(AccountItemModel account, Period period)
        {
            var isLeaf = !account.IsFolder;
           
            var trans = _dataModel.Transactions.Values.Where(t => period.Includes(t.Timestamp));
            foreach (var transaction in trans.Where(t=>t.Category == account))
            {

            }
        }
    }
}
