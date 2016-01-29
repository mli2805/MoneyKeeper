using System;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;
using Keeper.DomainModel.Transactions;
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

    public MonthAnalysisBlank FillInBlank(Saldo s, bool isMonthEnded)
    {
      Blank = new MonthAnalysisBlank();
      FillInBeginList(s);
      FillInIncomesList(s);
      FillInExpenseList(s);
      FillInEndList(s);
      FillInResultList(s);
      FillInDepositResultList(s);
      FillInRatesList(s);
      if (!isMonthEnded) CalculateForecast(s);
      return Blank;
    }

    private void FillInBeginList(Saldo s)
    {
      Blank.BeforeList = FillListWithDateBalanceInCurrencies(s.BeginBalance.Common, s.StartDate, "Входящий остаток на начало месяца");
      Blank.BeforeListOnHands = FillListWithDateBalanceInCurrencies(s.BeginBalance.OnHands, s.StartDate, "На руках                       ");
      Blank.BeforeListOnDeposits = FillListWithDateBalanceInCurrencies(s.BeginBalance.OnDeposits, s.StartDate, "Депозиты");
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

    private void FillInIncomesList(Saldo s)
    {
      Blank.IncomesToHandsList = new ObservableCollection<string> { String.Format("Доходы на руки  {0:#,0} usd\n", s.Incomes.OnHands.TotalInUsd) };
      foreach (var transaction in s.Incomes.OnHands.Transactions)
      {
        Blank.IncomesToHandsList.Add(TransactionForIncomesList(transaction));
      }

      Blank.IncomesToDepositsList = new ObservableCollection<string> { String.Format("Проценты по депозитам   {0:#,0} usd\n", s.Incomes.OnDeposits.TotalInUsd) };
      foreach (var transaction in s.Incomes.OnDeposits.Transactions)
      {
        Blank.IncomesToDepositsList.Add(TransactionForIncomesList(transaction));
      }

      Blank.IncomesTotal = new ObservableCollection<string> 
       { string.Format("                                                                          Всего доходы  {0:#,0} usd", s.Incomes.TotalInUsd) };
    }

    private string TransactionForIncomesList(Transaction tr)
    {
      var shortDepositName = tr.Credit.Deposit == null ? "" : tr.Credit.Deposit.ShortName; 
      return (tr.Currency == CurrencyCodes.USD) ?
             String.Format("{1:#,0}  {2}  {3} {4} {5}, {0:d MMM}",
             tr.Timestamp, tr.Amount, tr.Currency.ToString().ToLower(),
             (tr.Article.Name == "Проценты по депозитам") ? "" : tr.Article.Name, shortDepositName, tr.Comment) :
             String.Format("{1:#,0}  {2}  (= {3:#,0} $)  {4} {5} {6}, {0:d MMM}",
             tr.Timestamp, tr.Amount, tr.Currency.ToString().ToLower(),
             _rateExtractor.GetUsdEquivalent(tr.Amount, tr.Currency, tr.Timestamp),
             (tr.Article.Name == "Проценты по депозитам") ? "" : tr.Article.Name, shortDepositName, tr.Comment);
    }

    private void FillInExpenseList(Saldo s)
    {
      Blank.ExpenseList = new ObservableCollection<string> { "Расходы за месяц\n" };
      foreach (var category in s.Expense.Categories)
      {
        Blank.ExpenseList.Add(String.Format("{0:#,0} $ - {1}", category.Amount, category.MyAccount.Name));
      }
      Blank.ExpenseList.Add(String.Format("\nИтого {0:#,0} usd", s.Expense.TotalInUsd));

      Blank.LargeExpenseList = new ObservableCollection<string> { "В том числе крупные траты этого месяца\n" };
      foreach (var largeTransaction in s.Expense.LargeTransactions)
      {
        Blank.LargeExpenseList.Add(TransactionForLargeExpenseList(largeTransaction));
      }
      if (s.Expense.TotalForLargeInUsd == 0) Blank.LargeExpenseList[0] = "Крупных трат в этом месяце не было\n";
      else
      {
        Blank.LargeExpenseList.Add(String.Format("\nИтого крупных {0:#,0} usd", s.Expense.TotalForLargeInUsd));
        Blank.LargeExpenseList.Add(String.Format("\nТекущие расходы {0:#,0} usd", s.Expense.TotalInUsd - s.Expense.TotalForLargeInUsd));
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

    private void FillInEndList(Saldo s)
    {
      Blank.AfterList = FillListWithDateBalanceInCurrencies(s.EndBalance.Common, s.StartDate.AddMonths(1), "Исходящий остаток на конец месяца");
      Blank.AfterListOnHands = FillListWithDateBalanceInCurrencies(s.EndBalance.OnHands, s.StartDate.AddMonths(1), "На руках                         ");
      Blank.AfterListOnDeposits = FillListWithDateBalanceInCurrencies(s.EndBalance.OnDeposits, s.StartDate.AddMonths(1), "Депозиты");
      // если не добавить день - получишь остаток на утро последнего дня
    }

    private void FillInResultList(Saldo s)
    {
      Blank.ResultList = new ObservableCollection<string> {
        String.Format( "Финансовый результат месяца {0:#,0} - {1:#,0} = {2:#,0} usd\n",
        s.Incomes.TotalInUsd, s.Expense.TotalInUsd, s.Incomes.TotalInUsd - s.Expense.TotalInUsd)};

      Blank.ResultList.Add(String.Format("Курсовые разницы {4:#,0} - ({0:#,0} + {1:#,0} - {2:#,0}) = {3:#,0} usd",
                                         s.BeginBalance.Common.TotalInUsd, s.Incomes.TotalInUsd, s.Expense.TotalInUsd, s.ExchangeDifference, s.EndBalance.Common.TotalInUsd));

      Blank.ResultList.Add(String.Format("\n\n\nС учетом курсовых разниц {0:#,0} - {1:#,0} + {2:#,0} = {3:#,0} usd",
             s.Incomes.TotalInUsd, s.Expense.TotalInUsd, s.ExchangeDifference, s.EndBalance.Common.TotalInUsd - s.BeginBalance.Common.TotalInUsd));
    }

    private void FillInDepositResultList(Saldo s)
    {
      Blank.DepositResultList = new ObservableCollection<string> {
                                  String.Format("Открытие или доп.взносы {0:#,0} usd", s.TransferToDeposit)};
      Blank.DepositResultList.Add(String.Format("Закрытие или расходные {0:#,0} usd", s.TransferFromDeposit));
      Blank.DepositResultList.Add(String.Format("\nПроценты по депозитам (*)  {0:#,0} usd", s.Incomes.OnDeposits.TotalInUsd));
      Blank.DepositResultList.Add(String.Format("Курсовые разницы  {0:#,0} usd", s.ExchangeDepositDifference));
      Blank.DepositResultList.Add(String.Format("\nПрибыль с учетом курсовых разниц {0:#,0} usd",
        s.Incomes.OnDeposits.TotalInUsd + s.ExchangeDepositDifference));
    }

    private string HowCurrencyChanged(double a, double b)
    {
      if (a < b) return string.Format("упал");
      if (a > b) return string.Format("вырос");
      return string.Format("без изменений");
    }

    private void FillInRatesList(Saldo s)
    {
      var byrBegin = s.BeginRates.First(t => t.Currency == CurrencyCodes.BYR).Rate;
      var byrEnd = s.EndRates.First(t => t.Currency == CurrencyCodes.BYR).Rate;
      var euroBegin = 1/s.BeginRates.First(t => t.Currency == CurrencyCodes.EUR).Rate;
      var euroEnd = 1/s.EndRates.First(t => t.Currency == CurrencyCodes.EUR).Rate;
      var rurBegin = s.BeginRates.First(t => t.Currency == CurrencyCodes.RUB).Rate;
      var rurEnd = s.EndRates.First(t => t.Currency == CurrencyCodes.RUB).Rate;
      Blank.RatesList = new ObservableCollection<string>
         { String.Format( "Изменение курсов\n\n  Byr {0}:  {1:#,0} - {2:#,0}\n  Euro {3}:  {4:0.###} - {5:0.###} \n  Rur {6}:  {7:0.###} - {8:0.###} \n",
          HowCurrencyChanged(byrBegin, byrEnd),
          byrBegin,
          byrEnd,
          HowCurrencyChanged(euroEnd, euroBegin),
          euroBegin,
          euroEnd,
          HowCurrencyChanged(rurBegin, rurEnd),
          rurBegin,
          rurEnd)};
    }

    private void CalculateForecast(Saldo s)
    {
      Blank.ForecastListIncomes = new ObservableCollection<string> { "Прогноз доходов           \n" };
      foreach (var income in s.ForecastRegularIncome.Payments)
      {
        Blank.ForecastListIncomes.Add(String.Format("{0:#,0} {1} {2}", income.Amount, income.Currency.ToString().ToLower(), income.ArticleName));
      }
      if (s.ForecastRegularIncome.EstimatedSum > 0)
        Blank.ForecastListIncomes.Add(String.Format("\nЕще ожидается  {0:#,0} usd", s.ForecastRegularIncome.EstimatedSum));
      Blank.ForecastListIncomes.Add(String.Format("\nИтого  {0:#,0} usd", s.ForecastRegularIncome.TotalInUsd));

      var daysInMonth = s.StartDate.AddMonths(1).AddDays(-1).Day;
      var passedDays = DateTime.Today.Year == s.StartDate.Year && DateTime.Today.Month == s.StartDate.Month
                         ? DateTime.Today.Day
                         : daysInMonth;
      Blank.ForecastListExpense = new ObservableCollection<string> { "Прогноз расходов\n" };
      var averageExpenseInUsd = (s.Expense.TotalInUsd - s.Expense.TotalForLargeInUsd) / passedDays;
      Blank.ForecastListExpense.Add(String.Format("Среднедневные расходы {0:#,0} usd ( {1:#,0} byr)",
                                                  averageExpenseInUsd, averageExpenseInUsd * (decimal)s.EndRates.First(c => c.Currency == CurrencyCodes.BYR).Rate));
      Blank.ForecastListExpense.Add(String.Format("За {0} дней составит: {1:#,0} usd ( {2:#,0} byr)",
                                                  daysInMonth, averageExpenseInUsd * daysInMonth, averageExpenseInUsd * (decimal)s.EndRates.First(c => c.Currency == CurrencyCodes.BYR).Rate * daysInMonth));
      Blank.ForecastListExpense.Add(String.Format(" + крупные расходы:  {0:#,0} usd", s.Expense.TotalForLargeInUsd));
      s.ForecastExpense = averageExpenseInUsd * daysInMonth + s.Expense.TotalForLargeInUsd;
      Blank.ForecastListExpense.Add(String.Format("\nИтого расходов {0:#,0} usd ", s.ForecastExpense));

      Blank.ForecastListBalance = new ObservableCollection<string>
                                    {
                                      "Прогноз результата\n\n",
                                      String.Format("Финансовый результат  {0:#,0} usd", s.ForecastFinResult),
                                      String.Format(" (с учетом курсовых разниц  {0:#,0} usd)\n", s.ForecastFinResult+s.ExchangeDifference),
                                      String.Format("Исходящий остаток  {0:#,0} usd", s.ForecastEndBalance),
                                      String.Format(" (с учетом курсовых разниц  {0:#,0} usd)", s.ForecastEndBalance+s.ExchangeDifference)
                                    };
    }
  }
}