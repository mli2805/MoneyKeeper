using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018.ExpensesOnAccount
{
    public class ExpensesOnAccountViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private string _caption;
        private List<TranLine> _transactions;
        public List<CategoryLine> Incomes { get; set; }
        public List<CategoryLine> Expenses { get; set; }

        public List<TranLine> Transactions
        {
            get => _transactions.OrderByDescending(t=>t.Timestamp).ToList();
            set => _transactions = value;
        }

        public string ExpensesTotalStr { get; set; }
        public string IncomesTotalStr { get; set; }
        public string PeriodStr { get; set; }
        public ExpensesOnAccountViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _caption;
        }

        public void Initialize(AccountItemModel accountItemModel, Period period)
        {
            _caption = $"{accountItemModel.Name}";
            _transactions = new List<TranLine>();

            Expenses = InitializeCollection(accountItemModel, period, OperationType.Расход)
                .Collection.OrderByDescending(c => c.Total).ToList();
            ExpensesTotalStr = $"{Expenses.Sum(l => l.Total):#,0.##}";

            Incomes = InitializeCollection(accountItemModel, period, OperationType.Доход)
                .Collection.OrderByDescending(c => c.Total).ToList();
            IncomesTotalStr = $"{Incomes.Sum(l => l.Total):#,0.##}";

            PeriodStr = $"{period}  (Период может быть выбран на главной форме)";
        }

        private CategoriesCollection InitializeCollection(AccountItemModel accountItemModel, Period period, OperationType operationType)
        {
            var categoriesCollection = new CategoriesCollection();

            var trans = _dataModel.Transactions.Values.OrderBy(t => t.Timestamp)
                .Where(t => t.MyAccount.Id == accountItemModel.Id
                            && t.Operation == operationType
                            && period.Includes(t.Timestamp)).ToArray();

            foreach (var transaction in trans)
            {
                var catId = transaction.GetTransactionBaseCategory(_dataModel, operationType);
                var catAccount = _dataModel.AcMoDict[catId];
                categoriesCollection.Add(catAccount.Name, transaction.Amount);
                _transactions.Add(new TranLine()
                {
                    Timestamp = transaction.Timestamp,
                    Category = catAccount.Name,
                    CounterpartyName = transaction.GetCounterpartyName(_dataModel),
                    Amount = transaction.Amount,
                    Currency = transaction.Currency,
                    Comment = transaction.Comment,
                    ColorStr = operationType == OperationType.Доход ? "Blue" : "Red",
                });
            }

            return categoriesCollection;
        }
    }
}
