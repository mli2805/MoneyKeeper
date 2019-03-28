using Caliburn.Micro;

namespace Keeper2018
{
    public class ExpenseByCategoriesViewModel : Screen
    {
        private readonly CategoriesDataExtractor _categoriesDataExtractor;

        public ExpenseByCategoriesViewModel(CategoriesDataExtractor categoriesDataExtractor)
        {
            _categoriesDataExtractor = categoriesDataExtractor;
        }

        public void Initialize()
        {
            var data = _categoriesDataExtractor.GetExpenseGrouppedByCategoryAndMonth();
        }
    }
}
