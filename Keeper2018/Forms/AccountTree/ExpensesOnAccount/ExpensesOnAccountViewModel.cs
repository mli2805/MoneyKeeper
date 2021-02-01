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
        public List<ExpenseLine> Categories { get; set; }
        public List<TranLine> Transactions { get; set; }
        public string TotalStr { get; set; }
        public string PeriodStr { get; set; }
        public ExpensesOnAccountViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _caption;
        }

        public void Initialize(AccountModel accountModel, Period period)
        {
            var expenses = new ExpenseCollection();
            Transactions = new List<TranLine>();

            var trans = _dataModel.Bin.Transactions.Values.OrderBy(t => t.Timestamp)
                .Where(t => t.MyAccount == accountModel.Id 
                            && t.Operation == OperationType.Расход
                            && period.Includes(t.Timestamp)).ToArray();

            foreach (var transaction in trans)
            {
                var catId = transaction.GetTransactionExpenseCategory(_dataModel);
                var catAccount = _dataModel.AcMoDict[catId];
                expenses.Add(catAccount.Name, transaction.Amount);
                Transactions.Add(new TranLine()
                {
                    Timestamp = transaction.Timestamp,
                    Category = catAccount.Name,
                    CounterpartyName = transaction.GetCounterpartyName(_dataModel),
                    Amount = transaction.Amount, 
                    Currency = transaction.Currency, 
                    Comment = transaction.Comment,
                });
            }

            Categories = expenses.Collection.OrderByDescending(c=>c.Total).ToList();

            _caption = $"{accountModel.Name}";
            TotalStr = $"{expenses.Collection.Sum(l => l.Total):#,0.##}";
            PeriodStr = $"{period}  (Период может быть выбран на главной форме)";
        }
    }
}
