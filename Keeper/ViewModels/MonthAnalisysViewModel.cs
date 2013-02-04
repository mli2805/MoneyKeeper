using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils;

namespace Keeper.ViewModels
{
  class MonthAnalisysViewModel : Screen
  {
    public static KeeperTxtDb Db { get { return IoC.Get<KeeperTxtDb>(); } }

    private ObservableCollection<string> _beforeList;
    private ObservableCollection<string> _incomesList;
    private ObservableCollection<string> _expenseList;
    private ObservableCollection<string> _largeExpenseList;
    private ObservableCollection<string> _afterList;
    private ObservableCollection<string> _resultList;

    private DateTime _startDate;
    private Brush _resultForeground;

    public ObservableCollection<string> BeforeList
    {
      get { return _beforeList; }
      set
      {
        if (Equals(value, _beforeList)) return;
        _beforeList = value;
        NotifyOfPropertyChange(() => BeforeList);
      }
    }
    public ObservableCollection<string> IncomesList
    {
      get { return _incomesList; }
      set
      {
        if (Equals(value, _incomesList)) return;
        _incomesList = value;
        NotifyOfPropertyChange(() => IncomesList);
      }
    }
    public ObservableCollection<string> ExpenseList
    {
      get { return _expenseList; }
      set
      {
        if (Equals(value, _expenseList)) return;
        _expenseList = value;
        NotifyOfPropertyChange(() => ExpenseList);
      }
    }
    public ObservableCollection<string> LargeExpenseList
    {
      get { return _largeExpenseList; }
      set
      {
        if (Equals(value, _largeExpenseList)) return;
        _largeExpenseList = value;
        NotifyOfPropertyChange(() => LargeExpenseList);
      }
    }
    public ObservableCollection<string> AfterList
    {
      get { return _afterList; }
      set
      {
        if (Equals(value, _afterList)) return;
        _afterList = value;
        NotifyOfPropertyChange(() => AfterList);
      }
    }
    public ObservableCollection<string> ResultList
    {
      get { return _resultList; }
      set
      {
        if (Equals(value, _resultList)) return;
        _resultList = value;
        NotifyOfPropertyChange(() => ResultList);
      }
    }

    public DateTime StartDate
    {
      get { return _startDate; }
      set 
      {
        _startDate = value;
        AnalyzedPeriod = new Period(StartDate.Date, StartDate.Date.AddMonths(1).AddMinutes(-1));
        MonthAnalisysViewCaption = String.Format("Анализ месяца [{0}]", AnalyzedMonth);
        NotifyOfPropertyChange(()=>MonthAnalisysViewCaption);
      }
    }
    public string AnalyzedMonth
    {
      get { return String.Format("{0:MMMM yyyy}", StartDate); }
    }
    public string MonthAnalisysViewCaption { get; set; }
    public Period AnalyzedPeriod { get; set; }
    public Saldo MonthSaldo { get; set; }
    public Brush ResultForeground
    {
      get { return _resultForeground; }
      set
      {
        if (Equals(value, _resultForeground)) return;
        _resultForeground = value;
        NotifyOfPropertyChange(() => ResultForeground);
      }
    }

    public MonthAnalisysViewModel()
    {
      StartDate = new DateTime(2012, 7, 1);
      Calculate();
    }

    protected override void OnViewLoaded(object view)
    {
    }

    private void Calculate()
    {
      MonthSaldo = new Saldo();
      CalculateBeginBalance();
      CalculateIncomes();
      CalculateExpense();
      CalculateEndBalance();
      CalculateResult();
    }

    private void CalculateBeginBalance()
    {
      BeforeList = new ObservableCollection<string> { String.Format("Входящий остаток на начало месяца\n") };
      MonthSaldo.BeginBalance = FillListWithDateBalance(BeforeList, StartDate);
      MonthSaldo.BeginByrRate = (decimal)Rate.GetRate(CurrencyCodes.BYR, StartDate.AddDays(-1));
    }

    private decimal FillListWithDateBalance(ObservableCollection<string> list, DateTime date)
    {
      var myAccountsRoot = (from account in Db.Accounts
                            where account.Name == "Мои"
                            select account).FirstOrDefault();

      var startBalance = Balance.AccountBalancePairsBeforeDay(myAccountsRoot, date);
      decimal balanceInUsd = 0;
      foreach (var balancePair in startBalance)
      {
        if (balancePair.Amount == 0) continue;
        if (balancePair.Currency == CurrencyCodes.USD)
        {
          balanceInUsd += balancePair.Amount;
          list.Add(balancePair.ToString());
        }
        else
        {
          decimal amountInUsd;
          Rate.GetUsdEquivalent(balancePair.Amount, (CurrencyCodes)balancePair.Currency, date, out amountInUsd);
          balanceInUsd += amountInUsd;
          list.Add(String.Format("{0}  (= {1:#,0} $)", balancePair.ToString(), amountInUsd));
        }
      }
      list.Add(String.Format("\nИтого {0:#,0} usd", balanceInUsd));
      return balanceInUsd;
    }

    private void CalculateIncomes()
    {
      IncomesList = new ObservableCollection<string> { "Доходы за месяц\n" };
      decimal incomesInUsd = 0;
      var incomes = from t in Db.Transactions
                    where AnalyzedPeriod.IsDateTimeIn(t.Timestamp) && t.Operation == OperationType.Доход
                    select t;
      foreach (var transaction in incomes)
      {
        if (transaction.Currency == CurrencyCodes.USD)
        {
          incomesInUsd += transaction.Amount;
          IncomesList.Add(String.Format("{1:#,0}  {2}  {3} {4} , {0:d MMM}",
            transaction.Timestamp, transaction.Amount, transaction.Currency.ToString().ToLower(),
            transaction.Article, transaction.Comment));
        }
        else
        {
          decimal amountInUsd;
          Rate.GetUsdEquivalent(transaction.Amount, transaction.Currency, transaction.Timestamp, out amountInUsd);
          incomesInUsd += amountInUsd;
          IncomesList.Add(String.Format("{1:#,0}  {2}  (= {3:#,0} $)  {4} {5} , {0:d MMM}",
            transaction.Timestamp, transaction.Amount, transaction.Currency.ToString().ToLower(),
            amountInUsd, transaction.Article, transaction.Comment));
        }
      }
      IncomesList.Add(String.Format("\nИтого {0:#,0} usd", incomesInUsd));
      MonthSaldo.Incomes = incomesInUsd;
    }

    private void CalculateExpense()
    {
      ExpenseList = new ObservableCollection<string> { "Расходы за месяц\n" };
      LargeExpenseList = new ObservableCollection<string> { "В том числе крупные траты этого месяца\n" };
      var expenseTransactions = from t in Db.Transactions
                                where AnalyzedPeriod.IsDateTimeIn(t.Timestamp) && t.Operation == OperationType.Расход
                                select t;
      var expenseTransactionsInUsd =
        from t in expenseTransactions
        join r in Db.CurrencyRates
         on new { t.Timestamp.Date, t.Currency } equals new { r.BankDay.Date, r.Currency } into g
        from rate in g.DefaultIfEmpty()
        select new
        {
          t.Timestamp,
          t.Amount,
          t.Currency,
          t.Article,
          AmountInUsd = rate != null ? t.Amount / (decimal)rate.Rate : t.Amount,
          t.Comment
        };

      decimal expenseInUsd = 0;
      var expenseRoot = (from account in Db.Accounts
                         where account.Name == "Все расходы"
                         select account).First();
      foreach (var expense in expenseRoot.Children)
      {
        decimal amountInUsd = (from e in expenseTransactionsInUsd
                               where e.Article.IsTheSameOrDescendantOf(expense.Name)
                               select e).Sum(a => a.AmountInUsd);
        if (amountInUsd != 0) ExpenseList.Add(String.Format("{0:#,0} $ - {1}", -amountInUsd, expense.Name));
        expenseInUsd -= amountInUsd;
      }
      ExpenseList.Add(String.Format("\nИтого {0:#,0} usd", expenseInUsd));
      MonthSaldo.Expense = expenseInUsd;

      decimal largeExpenseInUsd = 0;
      foreach (var transaction in expenseTransactionsInUsd)
      {
        if (transaction.AmountInUsd < 50) continue;
        if (transaction.Currency == CurrencyCodes.USD)
          LargeExpenseList.Add(String.Format("               {1:#,0}  {2}  {3} {4} , {0:d MMM}",
            transaction.Timestamp, transaction.Amount, transaction.Currency.ToString().ToLower(),
            transaction.Article, transaction.Comment));
        else
          LargeExpenseList.Add(String.Format("{1:#,0}  {2}  (= {3:#,0} $)  {4} {5} , {0:d MMM}",
            transaction.Timestamp, transaction.Amount, transaction.Currency.ToString().ToLower(),
            transaction.AmountInUsd, transaction.Article, transaction.Comment));
        largeExpenseInUsd -= transaction.AmountInUsd;
      }
      if (largeExpenseInUsd == 0) LargeExpenseList[0] = "Крупных трат в этом месяце не было\n";
      else
      {
        LargeExpenseList.Add(String.Format("\nИтого крупных {0:#,0} usd", largeExpenseInUsd));
        LargeExpenseList.Add(String.Format("\nТекущие расходы {0:#,0} usd", expenseInUsd - largeExpenseInUsd));
      }
    }

    private void CalculateEndBalance()
    {
      AfterList = new ObservableCollection<string> { String.Format("Исходящий остаток на конец месяца\n") };
      MonthSaldo.EndBalance = FillListWithDateBalance(AfterList, StartDate.AddMonths(1));
      MonthSaldo.EndByrRate = (decimal)Rate.GetRate(CurrencyCodes.BYR, StartDate.AddMonths(1).AddDays(-1));
    }

    private void CalculateResult()
    {
      ResultForeground = MonthSaldo.BeginBalance > MonthSaldo.EndBalance ? Brushes.Red : Brushes.Blue;
      ResultList = new ObservableCollection<string> {String.Format( "Финансовый результат месяца {0:#,0} {1:#,0} = {2:#,0} usd\n",
                                      MonthSaldo.Incomes, MonthSaldo.Expense, MonthSaldo.SaldoIncomesExpense)};

      ResultList.Add(String.Format("Курсовые разницы {4:#,0} - ({0:#,0} - {1:#,0} + {2:#,0}) = {3:#,0} usd",
        MonthSaldo.BeginBalance, MonthSaldo.Incomes, -MonthSaldo.Expense, MonthSaldo.ExchangeDifference, MonthSaldo.EndBalance));
      ResultList.Add(String.Format("Курсы Byr/Usd на начало и конец месца:  {0:#,0} - {1:#,0} \n", MonthSaldo.BeginByrRate, MonthSaldo.EndByrRate));

      ResultList.Add(String.Format("С учетом курсовых разниц {0:#,0} - {1:#,0} - {2:#,0} = {3:#,0} usd",
        MonthSaldo.Incomes, -MonthSaldo.Expense, -MonthSaldo.ExchangeDifference, MonthSaldo.Result));
    }

    public void ShowPreviousMonth()
    {
      StartDate = StartDate.AddMonths(-1);
      Calculate();
    }

    public void ShowNextMonth()
    {
      StartDate = StartDate.AddMonths(1);
      Calculate();
    }
  }

}
