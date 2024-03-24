using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public class MonthIncome : IIncomeForPeriod 
    {
        private readonly Dictionary<IncomeCategories, List<Tuple<string, decimal>>> _dict =
            new Dictionary<IncomeCategories, List<Tuple<string, decimal>>>();

        public void Add(IncomeCategories branch, string line, decimal value)
        {
            if (!_dict.ContainsKey(branch))
                _dict[branch] = new List<Tuple<string, decimal>>();
            _dict[branch].Add(new Tuple<string, decimal>(line, value));
        }

        public void Fill(ListOfLines list)
        {
            foreach (var pair in _dict.OrderBy(p=>p.Key))
                list.InsertLinesIntoIncomeList(pair.Value, pair.Key.ToString());
        }

        public decimal GetTotal()
        {
            return _dict.Sum(v => v.Value.Sum(c => c.Item2));
        }
    }
}