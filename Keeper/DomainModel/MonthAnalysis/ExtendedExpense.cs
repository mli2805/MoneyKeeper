using System.Collections.Generic;
using Keeper.Utils.DiagramDataExtraction;

namespace Keeper.DomainModel.MonthAnalysis
{
    public class ExtendedExpense
    {
        public List<TranForAnalysis> LargeTransactions { get; set; }
        public List<CategoriesDataElement> Categories { get; set; }
        public decimal TotalForLargeInUsd { get; set; }
        public decimal TotalInUsd { get; set; }

        public ExtendedExpense()
        {
            LargeTransactions = new List<TranForAnalysis>();
            Categories = new List<CategoriesDataElement>();
        }
    }
}