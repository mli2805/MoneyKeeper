using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public class MonthIncome : IIncomeForPeriod 
    {
        public readonly Dictionary<IncomeCategories, List<Tuple<string, decimal>>> Dict =
            new Dictionary<IncomeCategories, List<Tuple<string, decimal>>>();

        public void Add(IncomeCategories branch, string line, decimal value)
        {
            if (!Dict.ContainsKey(branch))
                Dict[branch] = new List<Tuple<string, decimal>>();
            Dict[branch].Add(new Tuple<string, decimal>(line, value));
        }

        public decimal BranchTotal(IncomeCategories branch)
        {
            return Dict.TryGetValue(branch, out var value) ? value.Sum(c => c.Item2) : 0;
        }

        public void Fill(ListOfLines list)
        {
            foreach (var pair in Dict.OrderBy(p=>p.Key))
                list.InsertLinesIntoIncomeList(pair.Value, pair.Key.ToString());
        }

        public decimal GetTotal()
        {
            return Dict.Sum(v => v.Value.Sum(c => c.Item2));
        }
    }
}