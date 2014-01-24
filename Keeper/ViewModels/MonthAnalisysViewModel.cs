using System;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils.CommonKeeper;
using Keeper.Utils.Rates;

namespace Keeper.ViewModels
{
	[Export]
  class MonthAnalisysViewModel : Screen
  {
    private readonly RateExtractor _rateExtractor;
    private readonly KeeperDb _db;

    private readonly MonthAnalysisModel _monthAnalysisModel;

    private bool _isMonthEnded;

    #region Lists
    private ObservableCollection<string> _beforeList;
    private ObservableCollection<string> _beforeListOnHands;
    private ObservableCollection<string> _beforeListOnDeposits;
    private ObservableCollection<string> _incomesToHandsList;
    private ObservableCollection<string> _incomesToDepositsList;
    private ObservableCollection<string> _incomesTotal;
    private ObservableCollection<string> _expenseList;
    private ObservableCollection<string> _largeExpenseList;
    private ObservableCollection<string> _afterList;
    private ObservableCollection<string> _afterListOnHands;
    private ObservableCollection<string> _afterListOnDeposits;
    private ObservableCollection<string> _resultList;
    private ObservableCollection<string> _forecastListIncomes;
    private ObservableCollection<string> _forecastListExpense;
    private ObservableCollection<string> _forecastListBalance;


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
    public ObservableCollection<string> BeforeListOnHands
    {
      get { return _beforeListOnHands; }
      set
      {
        if (Equals(value, _beforeListOnHands)) return;
        _beforeListOnHands = value;
        NotifyOfPropertyChange(() => _beforeListOnHands);
      }
    }
    public ObservableCollection<string> BeforeListOnDeposits
    {
      get { return _beforeListOnDeposits; }
      set
      {
        if (Equals(value, _beforeListOnDeposits)) return;
        _beforeListOnDeposits = value;
        NotifyOfPropertyChange(() => BeforeListOnDeposits);
      }
    }
    public ObservableCollection<string> IncomesToHandsList
    {
      get { return _incomesToHandsList; }
      set
      {
        if (Equals(value, _incomesToHandsList)) return;
        _incomesToHandsList = value;
        NotifyOfPropertyChange(() => IncomesToHandsList);
      }
    }
    public ObservableCollection<string> IncomesTotal
    {
      get { return _incomesTotal; }
      set
      {
        if (Equals(value, _incomesTotal)) return;
        _incomesTotal = value;
        NotifyOfPropertyChange(() => IncomesTotal);
      }
    }
    public ObservableCollection<string> IncomesToDepositsList
    {
      get { return _incomesToDepositsList; }
      set
      {
        if (Equals(value, _incomesToDepositsList)) return;
        _incomesToDepositsList = value;
        NotifyOfPropertyChange(() => IncomesToDepositsList);
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
    public ObservableCollection<string> AfterListOnHands
    {
      get { return _afterListOnHands; }
      set
      {
        if (Equals(value, _afterListOnHands)) return;
        _afterListOnHands = value;
        NotifyOfPropertyChange(() => AfterListOnHands);
      }
    }
    public ObservableCollection<string> AfterListOnDeposits
    {
      get { return _afterListOnDeposits; }
      set
      {
        if (Equals(value, _afterListOnDeposits)) return;
        _afterListOnDeposits = value;
        NotifyOfPropertyChange(() => AfterListOnDeposits);
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
    #endregion

    private DateTime _startDate;
    private Brush _resultForeground;
    private Visibility _forecastListVisibility;

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
        if (AnalyzedPeriod.Contains(DateTime.Today))
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
    public MonthAnalisysViewModel(KeeperDb db, RateExtractor rateExtractor, MonthAnalysisModel monthAnalysisModel)
    {
      _db = db;
      _rateExtractor =rateExtractor;

      _monthAnalysisModel = monthAnalysisModel;
      MonthSaldo = _monthAnalysisModel.AnalizeMonth(DateTime.Today);
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
      BeforeList = FillListWithDateBalanceInCurrencies(MonthSaldo.BeginBalance.Common, MonthSaldo.StartDate, "Входящий остаток на начало месяца");
      BeforeListOnHands = FillListWithDateBalanceInCurrencies(MonthSaldo.BeginBalance.OnHands, MonthSaldo.StartDate, "На руках                       ");
      BeforeListOnDeposits = FillListWithDateBalanceInCurrencies(MonthSaldo.BeginBalance.OnDeposits, MonthSaldo.StartDate, "Депозиты");
    }

    private ObservableCollection<string> FillListWithDateBalanceInCurrencies(ExtendedBalance balance, DateTime date, string caption)
    {
      var content = new ObservableCollection<string>();
      content.Add(caption);
      content.Add("");
      foreach (var balancePair in balance.InCurrencies)    
      {
        if (balancePair.Amount == 0) continue;
        if (balancePair.Currency == CurrencyCodes.USD)
        {
          content.Add(balancePair.ToString());
        }
        else
        {                                                          
          decimal amountInUsd = _rateExtractor.GetUsdEquivalent(balancePair.Amount, balancePair.Currency, date.AddDays(-1));
          content.Add(String.Format("{0}  (= {1:#,0} $)", balancePair.ToString(), amountInUsd));
        }
      }
      content.Add("");
      content.Add(String.Format("Итого {0:#,0} usd", balance.TotalInUsd));

      return content;
    }

    private void FillInIncomesList()
    {
      IncomesToHandsList = new ObservableCollection<string> { String.Format("Доходы на руки  {0:#,0} usd\n", MonthSaldo.Incomes.OnHands.TotalInUsd) };
      foreach (var transaction in MonthSaldo.Incomes.OnHands.Transactions)
      {
        IncomesToHandsList.Add(TransactionForIncomesList(transaction));
      }

      IncomesToDepositsList = new ObservableCollection<string> { String.Format("Доходы по депозитам   {0:#,0} usd\n", MonthSaldo.Incomes.OnDeposits.TotalInUsd) };
      foreach (var transaction in MonthSaldo.Incomes.OnDeposits.Transactions)
      {
        IncomesToDepositsList.Add(TransactionForIncomesList(transaction));
      }

      IncomesTotal = new ObservableCollection<string>() { string.Format("                                       Всего доходы  {0:0,0} usd",MonthSaldo.Incomes.TotalInUsd)};
    }

	  private string TransactionForIncomesList(Transaction transaction)
	  {
	    if (transaction.Currency == CurrencyCodes.USD)
	    {
	      return String.Format("{1:#,0}  {2}  {3} {4} , {0:d MMM}",
	                                    transaction.Timestamp, transaction.Amount, transaction.Currency.ToString().ToLower(),
	                                    transaction.Article, transaction.Comment);
	    }
	    else
	    {
	      decimal amountInUsd;
	      _rateExtractor.GetUsdEquivalentString(transaction.Amount, transaction.Currency, transaction.Timestamp,
	                                            out amountInUsd);
	      return String.Format("{1:#,0}  {2}  (= {3:#,0} $)  {4} {5} , {0:d MMM}",
	                                    transaction.Timestamp, transaction.Amount, transaction.Currency.ToString().ToLower(),
	                                    amountInUsd, transaction.Article, transaction.Comment);
	    }
	  }

	  private void FillInExpenseList()
    {
      ExpenseList = new ObservableCollection<string> { "Расходы за месяц\n" };
      LargeExpenseList = new ObservableCollection<string> { "В том числе крупные траты этого месяца\n" };
      var expenseTransactions = from t in _db.Transactions
                                where AnalyzedPeriod.Contains(t.Timestamp) && t.Operation == OperationType.Расход
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
                               where e.Article.Is(expense.Name)
                               select e).Sum(a => a.AmountInUsd);
        if (amountInUsd != 0) ExpenseList.Add(String.Format("{0:#,0} $ - {1}", amountInUsd, expense.Name));
      }
      ExpenseList.Add(String.Format("\nИтого {0:#,0} usd", MonthSaldo.Expense.TotalInUsd));

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
      if (MonthSaldo.Expense.TotalForLargeInUsd == 0) LargeExpenseList[0] = "Крупных трат в этом месяце не было\n";
      else
      {
        LargeExpenseList.Add(String.Format("\nИтого крупных {0:#,0} usd", MonthSaldo.Expense.TotalForLargeInUsd));
        LargeExpenseList.Add(String.Format("\nТекущие расходы {0:#,0} usd", MonthSaldo.Expense.TotalInUsd - MonthSaldo.Expense.TotalForLargeInUsd));
      }
    }

    private void FillInEndList()
    {
      AfterList = FillListWithDateBalanceInCurrencies(MonthSaldo.EndBalance.Common, MonthSaldo.StartDate.AddMonths(1), "Исходящий остаток на конец месяца");
      AfterListOnHands = FillListWithDateBalanceInCurrencies(MonthSaldo.EndBalance.OnHands, MonthSaldo.StartDate.AddMonths(1), "На руках                         ");
      AfterListOnDeposits = FillListWithDateBalanceInCurrencies(MonthSaldo.EndBalance.OnDeposits, MonthSaldo.StartDate.AddMonths(1), "Депозиты");
                                            // если не добавить день - получишь остаток на утро последнего дня
    }  

    private void FillInResultList()
    {
      ResultForeground = MonthSaldo.BeginBalance.Common.TotalInUsd > MonthSaldo.EndBalance.Common.TotalInUsd ? Brushes.Red : Brushes.Blue;
      ResultList = new ObservableCollection<string> {String.Format( "Финансовый результат месяца {0:#,0} - {1:#,0} = {2:#,0} usd\n",
                                      MonthSaldo.Incomes.TotalInUsd, MonthSaldo.Expense.TotalInUsd, MonthSaldo.SaldoIncomesExpense)};

      ResultList.Add(String.Format("Курсовые разницы {4:#,0} - ({0:#,0} + {1:#,0} - {2:#,0}) = {3:#,0} usd (с плюсом - в мою пользу)",
        MonthSaldo.BeginBalance.Common.TotalInUsd, MonthSaldo.Incomes.TotalInUsd, MonthSaldo.Expense.TotalInUsd, MonthSaldo.ExchangeDifference, MonthSaldo.EndBalance.Common.TotalInUsd));
      ResultList.Add(String.Format("Курсы на начало и конец месца  Byr/Usd:  {0:#,0} - {1:#,0} ;  Usd/Euro:  {2:0.###} - {3:0.###} \n", 
        MonthSaldo.BeginRates.First(t => t.Currency == CurrencyCodes.BYR).Rate, 
        MonthSaldo.EndRates.First(t => t.Currency == CurrencyCodes.BYR).Rate,
        1/MonthSaldo.BeginRates.First(t => t.Currency == CurrencyCodes.EUR).Rate, 
        1/MonthSaldo.EndRates.First(t => t.Currency == CurrencyCodes.EUR).Rate));

      ResultList.Add(String.Format("С учетом курсовых разниц {0:#,0} - {1:#,0} + {2:#,0} = {3:#,0} usd",
        MonthSaldo.Incomes.TotalInUsd, MonthSaldo.Expense.TotalInUsd, MonthSaldo.ExchangeDifference, MonthSaldo.Result));
    }

    private void CalculateForecast()
    {
      ForecastListIncomes = new ObservableCollection<string> { "Прогноз доходов            \n\n\n\n\n" };
      MonthSaldo.ForecastIncomes = MonthSaldo.Incomes.TotalInUsd;
      ForecastListIncomes.Add(String.Format("  {0:#,0} usd", MonthSaldo.ForecastIncomes));

      var daysInMonth = StartDate.AddMonths(1).AddDays(-1).Day;
      var passedDays = DateTime.Today.Year == StartDate.Year && DateTime.Today.Month == StartDate.Month
                         ? DateTime.Today.Day
                         : daysInMonth;
      ForecastListExpense = new ObservableCollection<string> { "Прогноз расходов\n" };
      var averageExpenseInUsd = (MonthSaldo.Expense.TotalInUsd - MonthSaldo.Expense.TotalForLargeInUsd) / passedDays;
      ForecastListExpense.Add(String.Format("Среднедневные расходы {0:#,0} usd ( {1:#,0} byr)",
        averageExpenseInUsd, averageExpenseInUsd *  (decimal)MonthSaldo.EndRates.First(c => c.Currency == CurrencyCodes.BYR).Rate ));
      ForecastListExpense.Add(String.Format("За {0} дней составит: {1:#,0} usd ( {2:#,0} byr)",
        daysInMonth, averageExpenseInUsd * daysInMonth, averageExpenseInUsd *  (decimal)MonthSaldo.EndRates.First(c => c.Currency == CurrencyCodes.BYR).Rate * daysInMonth ));
      ForecastListExpense.Add(String.Format(" + крупные расходы:  {0:#,0} usd", MonthSaldo.Expense.TotalForLargeInUsd));
      MonthSaldo.ForecastExpense = averageExpenseInUsd * daysInMonth + MonthSaldo.Expense.TotalForLargeInUsd;
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
      MonthSaldo = _monthAnalysisModel.AnalizeMonth(MonthSaldo.StartDate.AddMonths(-1));
      StartDate = MonthSaldo.StartDate; 
      FillInLists();
    }

    public void ShowNextMonth()
    {
      MonthSaldo = _monthAnalysisModel.AnalizeMonth(MonthSaldo.StartDate.AddMonths(1));
      StartDate = MonthSaldo.StartDate;
      FillInLists();
    }

    public void CloseView()
    {
      TryClose();
    }
  }

}
