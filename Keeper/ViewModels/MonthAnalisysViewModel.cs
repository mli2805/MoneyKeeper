using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils;

namespace Keeper.ViewModels
{
	[Export]
  class MonthAnalisysViewModel : Screen
  {
    private readonly RateExtractor _rateExtractor;
    private readonly KeeperDb _db;

    private readonly MonthAnalizer _monthAnalizer;

    private bool _isMonthEnded;

    private ObservableCollection<string> _beforeList;
    private ObservableCollection<string> _beforeListByCurrency;
    private ObservableCollection<string> _incomesList;
    private ObservableCollection<string> _expenseList;
    private ObservableCollection<string> _largeExpenseList;
    private ObservableCollection<string> _afterList;
    private ObservableCollection<string> _afterListByCurrency;
    private ObservableCollection<string> _resultList;
    private ObservableCollection<string> _forecastListIncomes;
    private ObservableCollection<string> _forecastListExpense;
    private ObservableCollection<string> _forecastListBalance;

    private DateTime _startDate;
    private Brush _resultForeground;
    private Visibility _forecastListVisibility;

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
    public ObservableCollection<string> BeforeListByCurrency
    {
      get { return _beforeListByCurrency; }
      set
      {
        if (Equals(value, _beforeListByCurrency)) return;
        _beforeListByCurrency = value;
        NotifyOfPropertyChange(() => BeforeListByCurrency);
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
    public ObservableCollection<string> AfterListByCurrency
    {
      get { return _afterListByCurrency; }
      set
      {
        if (Equals(value, _afterListByCurrency)) return;
        _afterListByCurrency = value;
        NotifyOfPropertyChange(() => AfterListByCurrency);
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
    public ObservableCollection<string> ForecastListIncomes
    {
      get { return _forecastListIncomes; }
      set
      {
        if (Equals(value, _forecastListIncomes)) return;
        _forecastListIncomes = value;
        NotifyOfPropertyChange(() => ForecastListIncomes);
      }
    }
    public ObservableCollection<string> ForecastListExpense
    {
      get { return _forecastListExpense; }
      set
      {
        if (Equals(value, _forecastListExpense)) return;
        _forecastListExpense = value;
        NotifyOfPropertyChange(() => ForecastListExpense);
      }
    }
    public ObservableCollection<string> ForecastListBalance
    {
      get { return _forecastListBalance; }
      set
      {
        if (Equals(value, _forecastListBalance)) return;
        _forecastListBalance = value;
        NotifyOfPropertyChange(() => ForecastListBalance);
      }
    }

    public Visibility ForecastListVisibility
    {
      get { return _forecastListVisibility; }
      set
      {
        if (Equals(value, _forecastListVisibility)) return;
        _forecastListVisibility = value;
        NotifyOfPropertyChange(() => ForecastListVisibility);
      }
    }
    public DateTime StartDate
    {
      get { return _startDate; }
      set
      {
        _startDate = value;
        AnalyzedPeriod = new Period(StartDate.Date, StartDate.Date.AddMonths(1).AddMinutes(-1));
        if (AnalyzedPeriod.IsDateTimeIn(DateTime.Today))
        {
          _isMonthEnded = false;
          ForecastListVisibility = Visibility.Visible;
        }
        else
        {
          _isMonthEnded = true;
          ForecastListVisibility = Visibility.Collapsed;
        }
        MonthAnalisysViewCaption = String.Format("Анализ месяца [{0}]", AnalyzedMonth);
        if (!_isMonthEnded) MonthAnalisysViewCaption += " - текущий период!";
        NotifyOfPropertyChange(() => MonthAnalisysViewCaption);
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

	  [ImportingConstructor]
    public MonthAnalisysViewModel(KeeperDb db, RateExtractor rateExtractor, MonthAnalizer monthAnalizer)
    {
      _db = db;
      _rateExtractor =rateExtractor;

      _monthAnalizer = monthAnalizer;
      MonthSaldo = _monthAnalizer.AnalizeMonth(DateTime.Today);
      StartDate = MonthSaldo.StartDate;
      FillInLists();
    }

    private void FillInLists()
    {
      FillInBeginList();
      FillInIncomesList();
      FillInExpenseList();
      FillInEndList();
      FillInResultList();
      if (!_isMonthEnded) CalculateForecast();
    }

    private void FillInBeginList()
    {
      BeforeList = new ObservableCollection<string> { "Входящий остаток на начало месяца                \n"};
      BeforeListByCurrency = FillListWithDateBalance(MonthSaldo.BeginBalanceInCurrencies, MonthSaldo.StartDate);
      BeforeList.Add(String.Format("Итого {0:#,0} usd", MonthSaldo.BeginBalance));
    }

    private ObservableCollection<string> FillListWithDateBalance(List<MoneyPair> balanceInCurrencies, DateTime date)
    {
      var list = new ObservableCollection<string>();
      list.Add("В разрезе валют:");
      foreach (var balancePair in balanceInCurrencies)    
      {
        if (balancePair.Amount == 0) continue;
        if (balancePair.Currency == CurrencyCodes.USD)
        {
          list.Add(balancePair.ToString());
        }
        else
        {                                                          
          decimal amountInUsd = _rateExtractor.GetUsdEquivalent(balancePair.Amount, balancePair.Currency, date.AddDays(-1));
          list.Add(String.Format("{0}  (= {1:#,0} $)", balancePair.ToString(), amountInUsd));
        }
      }
      return list;
    }

    private void FillInIncomesList()
    {
      IncomesList = new ObservableCollection<string> { "Доходы за месяц\n" };
      var incomes = from transaction in _db.Transactions
                     where transaction.Operation == OperationType.Доход  && 
                       transaction.Timestamp.Month == MonthSaldo.StartDate.Month &&
                        transaction.Timestamp.Year == MonthSaldo.StartDate.Year
                     select transaction;
      foreach (var transaction in incomes)
      {
        if (transaction.Currency == CurrencyCodes.USD)
        {
          IncomesList.Add(String.Format("{1:#,0}  {2}  {3} {4} , {0:d MMM}",
            transaction.Timestamp, transaction.Amount, transaction.Currency.ToString().ToLower(),
            transaction.Article, transaction.Comment));
        }
        else
        {
          decimal amountInUsd;
          _rateExtractor.GetUsdEquivalentString(transaction.Amount, transaction.Currency, transaction.Timestamp, out amountInUsd);
          IncomesList.Add(String.Format("{1:#,0}  {2}  (= {3:#,0} $)  {4} {5} , {0:d MMM}",
            transaction.Timestamp, transaction.Amount, transaction.Currency.ToString().ToLower(),
            amountInUsd, transaction.Article, transaction.Comment));
        }
      }
      IncomesList.Add(String.Format("\nИтого {0:#,0} usd", MonthSaldo.Incomes));
    }

    private void FillInExpenseList()
    {
      ExpenseList = new ObservableCollection<string> { "Расходы за месяц\n" };
      LargeExpenseList = new ObservableCollection<string> { "В том числе крупные траты этого месяца\n" };
      var expenseTransactions = from t in _db.Transactions
                                where AnalyzedPeriod.IsDateTimeIn(t.Timestamp) && t.Operation == OperationType.Расход
                                orderby t.Timestamp
                                select t;


      var expenseTransactionsInUsd =
        from t in expenseTransactions
        join r in _db.CurrencyRates
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

      var expenseRoot = (from account in _db.Accounts
                         where account.Name == "Все расходы"
                         select account).First();
      foreach (var expense in expenseRoot.Children)
      {
        decimal amountInUsd = (from e in expenseTransactionsInUsd
                               where e.Article.IsTheSameOrDescendantOf(expense.Name)
                               select e).Sum(a => a.AmountInUsd);
        if (amountInUsd != 0) ExpenseList.Add(String.Format("{0:#,0} $ - {1}", amountInUsd, expense.Name));
      }
      ExpenseList.Add(String.Format("\nИтого {0:#,0} usd", MonthSaldo.Expense));

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
      }
      if (MonthSaldo.LargeExpense == 0) LargeExpenseList[0] = "Крупных трат в этом месяце не было\n";
      else
      {
        LargeExpenseList.Add(String.Format("\nИтого крупных {0:#,0} usd", MonthSaldo.LargeExpense));
        LargeExpenseList.Add(String.Format("\nТекущие расходы {0:#,0} usd", MonthSaldo.Expense - MonthSaldo.LargeExpense));
      }
    }

    private void FillInEndList()
    {
      AfterList = new ObservableCollection<string> { "Исходящий остаток на конец месяца                \n" };
      AfterListByCurrency = FillListWithDateBalance(MonthSaldo.EndBalanceInCurrencies, 
                         MonthSaldo.LastDayWithTransactionsInMonth.AddDays(1));
                                            // если не добавить день - получишь остаток на утро последнего дня
      AfterList.Add(String.Format("Итого {0:#,0} usd", MonthSaldo.EndBalance));
    }  

    private void FillInResultList()
    {
      ResultForeground = MonthSaldo.BeginBalance > MonthSaldo.EndBalance ? Brushes.Red : Brushes.Blue;
      ResultList = new ObservableCollection<string> {String.Format( "Финансовый результат месяца {0:#,0} - {1:#,0} = {2:#,0} usd\n",
                                      MonthSaldo.Incomes, MonthSaldo.Expense, MonthSaldo.SaldoIncomesExpense)};

      ResultList.Add(String.Format("Курсовые разницы {4:#,0} - ({0:#,0} + {1:#,0} - {2:#,0}) = {3:#,0} usd (с плюсом - в мою пользу)",
        MonthSaldo.BeginBalance, MonthSaldo.Incomes, MonthSaldo.Expense, MonthSaldo.ExchangeDifference, MonthSaldo.EndBalance));
      ResultList.Add(String.Format("Курсы Byr/Usd на начало и конец месца:  {0:#,0} - {1:#,0} \n", MonthSaldo.BeginByrRate, MonthSaldo.EndByrRate));

      ResultList.Add(String.Format("С учетом курсовых разниц {0:#,0} - {1:#,0} + {2:#,0} = {3:#,0} usd",
        MonthSaldo.Incomes, MonthSaldo.Expense, MonthSaldo.ExchangeDifference, MonthSaldo.Result));
    }

    private void CalculateForecast()
    {
      ForecastListIncomes = new ObservableCollection<string> { "Прогноз доходов            \n\n\n\n\n" };
      MonthSaldo.ForecastIncomes = MonthSaldo.Incomes;
      ForecastListIncomes.Add(String.Format("  {0:#,0} usd", MonthSaldo.ForecastIncomes));

      ForecastListExpense = new ObservableCollection<string> { "Прогноз расходов\n" };
      var averageExpenseInUsd = (MonthSaldo.Expense - MonthSaldo.LargeExpense)/MonthSaldo.LastDayWithTransactionsInMonth.Day;
      ForecastListExpense.Add(String.Format("Среднедневные расходы {0:#,0} usd ( {1:#,0} byr)",
        averageExpenseInUsd, averageExpenseInUsd *  MonthSaldo.EndByrRate ));
      var daysInMonth = StartDate.AddMonths(1).AddDays(-1).Day;
      ForecastListExpense.Add(String.Format("За {0} дней составит: {1:#,0} usd ( {2:#,0} byr)",
        daysInMonth, averageExpenseInUsd * daysInMonth, averageExpenseInUsd *  MonthSaldo.EndByrRate * daysInMonth ));
      ForecastListExpense.Add(String.Format(" + крупные расходы:  {0:#,0} usd", MonthSaldo.LargeExpense));
      MonthSaldo.ForecastExpense = averageExpenseInUsd*daysInMonth + MonthSaldo.LargeExpense;
      ForecastListExpense.Add(String.Format("\nИтого расходов {0:#,0} usd ", MonthSaldo.ForecastExpense));

      ForecastListBalance = new ObservableCollection<string>
                              {
                                "Прогноз результата\n\n",
                                String.Format("Финансовый результат  {0:#,0} usd\n\n", MonthSaldo.ForecastFinResult),
                                String.Format("Исходящий остаток  {0:#,0} usd", MonthSaldo.ForecastEndBalance)
                              };
    }

    public void ShowPreviousMonth()
    {
      MonthSaldo = _monthAnalizer.AnalizeMonth(MonthSaldo.StartDate.AddMonths(-1));
      StartDate = MonthSaldo.StartDate; 
      FillInLists();
    }

    public void ShowNextMonth()
    {
      MonthSaldo = _monthAnalizer.AnalizeMonth(MonthSaldo.StartDate.AddMonths(1));
      StartDate = MonthSaldo.StartDate;
      FillInLists();
    }

    public void CloseView()
    {
      TryClose();
    }
  }

}
