using System;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;
using Keeper.Models;
using Keeper.Utils.Rates;

namespace Keeper.Utils.MonthAnalysis
{
  [Export]
  public class MonthAnalysisBlankInscriber
  {
    private readonly RateExtractor _rateExtractor;

    private MonthAnalysisBlank Blank { get; set; }

    [ImportingConstructor]
    public MonthAnalysisBlankInscriber(RateExtractor rateExtractor)
    {
      _rateExtractor = rateExtractor;
    }

    public MonthAnalysisBlank FillInBlank(Saldo monthSaldo, bool isMonthEnded)
    {
      Blank = new MonthAnalysisBlank();
      FillInBeginList(monthSaldo);
      FillInIncomesList(monthSaldo);
      FillInExpenseList(monthSaldo);
      FillInEndList(monthSaldo);
      FillInResultList(monthSaldo);
      if (!isMonthEnded) CalculateForecast(monthSaldo);
      return Blank;
    }

    private void FillInBeginList(Saldo monthSaldo)
    {
      Blank.BeforeList = FillListWithDateBalanceInCurrencies(monthSaldo.BeginBalance.Common, monthSaldo.StartDate, "Входящий остаток на начало месяца");
      Blank.BeforeListOnHands = FillListWithDateBalanceInCurrencies(monthSaldo.BeginBalance.OnHands, monthSaldo.StartDate, "На руках                       ");
      Blank.BeforeListOnDeposits = FillListWithDateBalanceInCurrencies(monthSaldo.BeginBalance.OnDeposits, monthSaldo.StartDate, "Депозиты");
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

    private void FillInIncomesList(Saldo monthSaldo)
    {
      Blank.IncomesToHandsList = new ObservableCollection<string> { String.Format("Доходы на руки  {0:#,0} usd\n", monthSaldo.Incomes.OnHands.TotalInUsd) };
      foreach (var transaction in monthSaldo.Incomes.OnHands.Transactions)
      {
        Blank.IncomesToHandsList.Add(TransactionForIncomesList(transaction));
      }

      Blank.IncomesToDepositsList = new ObservableCollection<string> { String.Format("Доходы по депозитам   {0:#,0} usd\n", monthSaldo.Incomes.OnDeposits.TotalInUsd) };
      foreach (var transaction in monthSaldo.Incomes.OnDeposits.Transactions)
      {
        Blank.IncomesToDepositsList.Add(TransactionForIncomesList(transaction));
      }

      Blank.IncomesTotal = new ObservableCollection<string> { string.Format("                                       Всего доходы  {0:0,0} usd", monthSaldo.Incomes.TotalInUsd) };
    }

    private string TransactionForIncomesList(Transaction tr)
    {
      return (tr.Currency == CurrencyCodes.USD) ?
             String.Format("{1:#,0}  {2}  {3} {4} , {0:d MMM}",
             tr.Timestamp, tr.Amount, tr.Currency.ToString().ToLower(), tr.Article, tr.Comment) :
             String.Format("{1:#,0}  {2}  (= {3:#,0} $)  {4} {5} , {0:d MMM}",
             tr.Timestamp, tr.Amount, tr.Currency.ToString().ToLower(),
             _rateExtractor.GetUsdEquivalent(tr.Amount, tr.Currency, tr.Timestamp), tr.Article, tr.Comment);
    }

    private void FillInExpenseList(Saldo monthSaldo)
    {
      Blank.ExpenseList = new ObservableCollection<string> { "Расходы за месяц\n" };
      foreach (var category in monthSaldo.Expense.Categories)
      {
        Blank.ExpenseList.Add(String.Format("{0:#,0} $ - {1}", category.Amount, category.MyAccount.Name));
      }
      Blank.ExpenseList.Add(String.Format("\nИтого {0:#,0} usd", monthSaldo.Expense.TotalInUsd));

      Blank.LargeExpenseList = new ObservableCollection<string> { "В том числе крупные траты этого месяца\n" };
      foreach (var largeTransaction in monthSaldo.Expense.LargeTransactions)
      {
        Blank.LargeExpenseList.Add(TransactionForLargeExpenseList(largeTransaction));
      }
      if (monthSaldo.Expense.TotalForLargeInUsd == 0) Blank.LargeExpenseList[0] = "Крупных трат в этом месяце не было\n";
      else
      {
        Blank.LargeExpenseList.Add(String.Format("\nИтого крупных {0:#,0} usd", monthSaldo.Expense.TotalForLargeInUsd));
        Blank.LargeExpenseList.Add(String.Format("\nТекущие расходы {0:#,0} usd", monthSaldo.Expense.TotalInUsd - monthSaldo.Expense.TotalForLargeInUsd));
      }
    }

    private string TransactionForLargeExpenseList(ConvertedTransaction tr)
    {
      return (tr.Currency == CurrencyCodes.USD) ?
             String.Format("               {1:#,0}  {2}  {3} {4} , {0:d MMM}",
             tr.Timestamp, tr.Amount, tr.Currency.ToString().ToLower(), tr.Article, tr.Comment) :
             String.Format("{1:#,0}  {2}  (= {3:#,0} $)  {4} {5} , {0:d MMM}",
             tr.Timestamp, tr.Amount, tr.Currency.ToString().ToLower(), tr.AmountInUsd, tr.Article, tr.Comment);
    }

    private void FillInEndList(Saldo monthSaldo)
    {
      Blank.AfterList = FillListWithDateBalanceInCurrencies(monthSaldo.EndBalance.Common, monthSaldo.StartDate.AddMonths(1), "Исходящий остаток на конец месяца");
      Blank.AfterListOnHands = FillListWithDateBalanceInCurrencies(monthSaldo.EndBalance.OnHands, monthSaldo.StartDate.AddMonths(1), "На руках                         ");
      Blank.AfterListOnDeposits = FillListWithDateBalanceInCurrencies(monthSaldo.EndBalance.OnDeposits, monthSaldo.StartDate.AddMonths(1), "Депозиты");
      // если не добавить день - получишь остаток на утро последнего дня
    }

    private void FillInResultList(Saldo monthSaldo)
    {
      Blank.ResultList = new ObservableCollection<string> {String.Format( "Финансовый результат месяца {0:#,0} - {1:#,0} = {2:#,0} usd\n",
                                                                          monthSaldo.Incomes.TotalInUsd, monthSaldo.Expense.TotalInUsd, monthSaldo.SaldoIncomesExpense)};

      Blank.ResultList.Add(String.Format("Курсовые разницы {4:#,0} - ({0:#,0} + {1:#,0} - {2:#,0}) = {3:#,0} usd (с плюсом - в мою пользу)",
                                         monthSaldo.BeginBalance.Common.TotalInUsd, monthSaldo.Incomes.TotalInUsd, monthSaldo.Expense.TotalInUsd, monthSaldo.ExchangeDifference, monthSaldo.EndBalance.Common.TotalInUsd));
      Blank.ResultList.Add(String.Format("Курсы на начало и конец месца  Byr/Usd:  {0:#,0} - {1:#,0} ;  Usd/Euro:  {2:0.###} - {3:0.###} \n",
                                         monthSaldo.BeginRates.First(t => t.Currency == CurrencyCodes.BYR).Rate,
                                         monthSaldo.EndRates.First(t => t.Currency == CurrencyCodes.BYR).Rate,
                                         1 / monthSaldo.BeginRates.First(t => t.Currency == CurrencyCodes.EUR).Rate,
                                         1 / monthSaldo.EndRates.First(t => t.Currency == CurrencyCodes.EUR).Rate));

      Blank.ResultList.Add(String.Format("С учетом курсовых разниц {0:#,0} - {1:#,0} + {2:#,0} = {3:#,0} usd",
                                         monthSaldo.Incomes.TotalInUsd, monthSaldo.Expense.TotalInUsd, monthSaldo.ExchangeDifference, monthSaldo.Result));
    }

    private void CalculateForecast(Saldo monthSaldo)
    {
      Blank.ForecastListIncomes = new ObservableCollection<string> { "Прогноз доходов            \n\n\n\n\n" };
      monthSaldo.ForecastIncomes = monthSaldo.Incomes.TotalInUsd;
      Blank.ForecastListIncomes.Add(String.Format("  {0:#,0} usd", monthSaldo.ForecastIncomes));

      var daysInMonth = monthSaldo.StartDate.AddMonths(1).AddDays(-1).Day;
      var passedDays = DateTime.Today.Year == monthSaldo.StartDate.Year && DateTime.Today.Month == monthSaldo.StartDate.Month
                         ? DateTime.Today.Day
                         : daysInMonth;
      Blank.ForecastListExpense = new ObservableCollection<string> { "Прогноз расходов\n" };
      var averageExpenseInUsd = (monthSaldo.Expense.TotalInUsd - monthSaldo.Expense.TotalForLargeInUsd) / passedDays;
      Blank.ForecastListExpense.Add(String.Format("Среднедневные расходы {0:#,0} usd ( {1:#,0} byr)",
                                                  averageExpenseInUsd, averageExpenseInUsd * (decimal)monthSaldo.EndRates.First(c => c.Currency == CurrencyCodes.BYR).Rate));
      Blank.ForecastListExpense.Add(String.Format("За {0} дней составит: {1:#,0} usd ( {2:#,0} byr)",
                                                  daysInMonth, averageExpenseInUsd * daysInMonth, averageExpenseInUsd * (decimal)monthSaldo.EndRates.First(c => c.Currency == CurrencyCodes.BYR).Rate * daysInMonth));
      Blank.ForecastListExpense.Add(String.Format(" + крупные расходы:  {0:#,0} usd", monthSaldo.Expense.TotalForLargeInUsd));
      monthSaldo.ForecastExpense = averageExpenseInUsd * daysInMonth + monthSaldo.Expense.TotalForLargeInUsd;
      Blank.ForecastListExpense.Add(String.Format("\nИтого расходов {0:#,0} usd ", monthSaldo.ForecastExpense));

      Blank.ForecastListBalance = new ObservableCollection<string>
                                    {
                                      "Прогноз результата\n\n",
                                      String.Format("Финансовый результат  {0:#,0} usd\n\n", monthSaldo.ForecastFinResult),
                                      String.Format("Исходящий остаток  {0:#,0} usd", monthSaldo.ForecastEndBalance)
                                    };
    }
  }
}