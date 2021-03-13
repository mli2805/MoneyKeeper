using System.Collections.Generic;
using System.Linq;

namespace Keeper2018.ExpensesOnAccount
{
    public class CategoriesCollection
    {
        public List<CategoryLine> Collection { get; set; }

        public CategoriesCollection()
        {
            Collection = new List<CategoryLine>();
        }

        public void Add(string category, decimal amount)
        {
            var line = Collection.FirstOrDefault(l => l.Category == category);
            if (line != null)
                line.Total += amount;
            else
                Collection.Add(new CategoryLine() { Category = category, Total = amount });
        }

    }
}