using System.Collections.Generic;
using System.Linq;

namespace Keeper2018.ExpensesOnAccount
{
    public class ExpenseCollection
    {
        public List<ExpenseLine> Collection { get; set; }

        public ExpenseCollection()
        {
            Collection = new List<ExpenseLine>();
        }

        public void Add(string category, decimal amount)
        {
            var line = Collection.FirstOrDefault(l => l.Category == category);
            if (line != null)
                line.Total += amount;
            else
                Collection.Add(new ExpenseLine() { Category = category, Total = amount });
        }

    }
}