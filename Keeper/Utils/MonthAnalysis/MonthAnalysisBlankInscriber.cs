using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.MonthAnalysis;
using Keeper.DomainModel.WorkTypes;
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

        private ObservableCollection<string> FillListWithDateBalanceInCurrencies(MoneyBagWithTotal balance, DateTime date, string caption)
        {
            var content = new List<string>();
            content.Add(caption);
            content.Add("");
            content.AddRange(MoneyBagToListOfStrings(balance.MoneyBag, date));
            content.Add("");
            content.Add($"Итого {balance.TotalInUsd:#,0} usd");

            return new ObservableCollection<string>(content); 
        }
        private List<string> MoneyBagToListOfStrings(MoneyBag moneyBag, DateTime date)
        {
            var balanceList = new List<string>();
            var currencies = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();

            foreach (var currency in currencies)
            {
                if (moneyBag[currency] != 0)
                {
                    balanceList.Add(MoneyToString(new Money(currency, moneyBag[currency]), date));
                }
            }
            return balanceList;
        }

        private string MoneyToString(Money money, DateTime date)
        {
            decimal amountInUsd = money.Currency == CurrencyCodes.USD ? 1 : _rateExtractor.GetUsdEquivalent(money.Amount, money.Currency, date.AddDays(-1));
            switch (money.Currency)
            {
                    case CurrencyCodes.USD: return $"{money.Amount} usd";
                    case CurrencyCodes.BYR: return $"{money.Amount:#,#} byr (= {amountInUsd:#,0} $)";
                default: return $"{money.Amount:#,0.##} {money.Currency.ToString().ToLower()} (= {amountInUsd:#,0} $)";
            }
        }


        private void FillInIncomesList(Saldo s)
        {
            Blank.IncomesToHandsList = new ObservableCollection<string> {
                $"Доходы на руки  {s.Incomes.OnHands.TotalInUsd:#,0} usd\n"
            };
            foreach (var tran in s.Incomes.OnHands.Trans)
            {
                Blank.IncomesToHandsList.Add(TransactionForIncomesList(tran));
            }

            Blank.IncomesToDepositsList = new ObservableCollection<string> {
                $"Проценты по депозитам   {s.Incomes.OnDeposits.TotalInUsd:#,0} usd\n"
            };
            foreach (var tran in s.Incomes.OnDeposits.Trans)
            {
                Blank.IncomesToDepositsList.Add(TransactionForIncomesList(tran));
            }

            Blank.IncomesTotal = new ObservableCollection<string>
       {
           $"                                                                          Всего доходы  {s.Incomes.TotalInUsd:#,0} usd"
       };
        }

        private string TransactionForIncomesList(TranForAnalysis tr)
        {
            return (tr.Currency == CurrencyCodes.USD) ?
                   String.Format("{1:#,0}  {2}  {3} {4} {5}, {0:d MMM}",
                   tr.Timestamp, tr.Amount, tr.Currency.ToString().ToLower(),
                   (tr.Category.Name == "Проценты по депозитам") ? "" : tr.Category.Name, tr.DepoName, tr.Comment) :
                   String.Format("{1:#,0}  {2}  (= {3:#,0} $)  {4} {5} {6}, {0:d MMM}",
                   tr.Timestamp, tr.Amount, tr.Currency.ToString().ToLower(),
                   _rateExtractor.GetUsdEquivalent(tr.Amount, tr.Currency, tr.Timestamp),
                   (tr.Category.Name == "Проценты по депозитам") ? "" : tr.Category.Name, tr.DepoName, tr.Comment);
        }

        private void FillInExpenseList(Saldo s)
        {
            Blank.ExpenseList = new ObservableCollection<string> { "Расходы за месяц\n" };
            foreach (var dataElement in s.Expense.Categories)
            {
                Blank.ExpenseList.Add($"{dataElement.Amount:#,0} $ - {dataElement.Category}");
            }
            Blank.ExpenseList.Add($"\nИтого {s.Expense.TotalInUsd:#,0} usd");

            Blank.LargeExpenseList = new ObservableCollection<string> { "В том числе крупные траты этого месяца\n" };
            foreach (var largeTransaction in s.Expense.LargeTransactions)
            {
                Blank.LargeExpenseList.Add(TransactionForLargeExpenseList(largeTransaction));
            }
            if (s.Expense.TotalForLargeInUsd == 0) Blank.LargeExpenseList[0] = "Крупных трат в этом месяце не было\n";
            else
            {
                Blank.LargeExpenseList.Add($"\nИтого крупных {s.Expense.TotalForLargeInUsd:#,0} usd");
                Blank.LargeExpenseList.Add(
                    $"\nТекущие расходы {s.Expense.TotalInUsd - s.Expense.TotalForLargeInUsd:#,0} usd");
            }
        }

        private string TransactionForLargeExpenseList(TranForAnalysis tr)
        {
            return (tr.Currency == CurrencyCodes.USD) ?
                   string.Format("               {1:#,0}  {2}  {3} {4} , {0:d MMM}",
                   tr.Timestamp, tr.Amount, tr.Currency.ToString().ToLower(), tr.Category, tr.Comment) :
                   string.Format("{1:#,0}  {2}  (= {3:#,0} $)  {4} {5} , {0:d MMM}",
                   tr.Timestamp, tr.Amount, tr.Currency.ToString().ToLower(), tr.AmountInUsd, tr.Category, tr.Comment);
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
                $"Финансовый результат месяца {s.Incomes.TotalInUsd:#,0} - {s.Expense.TotalInUsd:#,0} = {s.Incomes.TotalInUsd - s.Expense.TotalInUsd:#,0} usd\n"
            };

            Blank.ResultList.Add(String.Format("Курсовые разницы {4:#,0} - ({0:#,0} + {1:#,0} - {2:#,0}) = {3:#,0} usd",
                                               s.BeginBalance.Common.TotalInUsd, s.Incomes.TotalInUsd, s.Expense.TotalInUsd, s.ExchangeDifference, s.EndBalance.Common.TotalInUsd));

            Blank.ResultList.Add(
                $"\n\n\nС учетом курсовых разниц {s.Incomes.TotalInUsd:#,0} - {s.Expense.TotalInUsd:#,0} + {s.ExchangeDifference:#,0} = {s.EndBalance.Common.TotalInUsd - s.BeginBalance.Common.TotalInUsd:#,0} usd");
        }

        private void FillInDepositResultList(Saldo s)
        {
            Blank.DepositResultList = new ObservableCollection<string> {
                $"Открытие или доп.взносы {s.DepoTraffic.ToDepo:#,0} usd"
            };
            Blank.DepositResultList.Add($"Закрытие или расходные {s.DepoTraffic.FromDepo:#,0} usd");
            Blank.DepositResultList.Add($"\nПроценты по депозитам (*)  {s.Incomes.OnDeposits.TotalInUsd:#,0} usd");
            Blank.DepositResultList.Add($"Курсовые разницы  {s.ExchangeDepositDifference:#,0} usd");
            Blank.DepositResultList.Add(
                $"\nПрибыль с учетом курсовых разниц {s.Incomes.OnDeposits.TotalInUsd + s.ExchangeDepositDifference:#,0} usd");
        }

        private string HowCurrencyChanged(double a, double b)
        {
            if (a < b) return "упал";
            if (a > b) return "вырос";
            return "без изменений";
        }

        private string HowBelarussianCurrencyChanged(Saldo s)
        {
            if (s.StartDate < new DateTime(2016, 7, 1))
            {
                var byrBegin = s.BeginRates.First(t => t.Currency == CurrencyCodes.BYR).Rate;
                var byrEnd = s.EndRates.First(t => t.Currency == CurrencyCodes.BYR).Rate;
                return $"Byr {HowCurrencyChanged(byrBegin, byrEnd)}:  {byrBegin:#,0} - {byrEnd:#,0}";
            }
            else
            {
                var bynBegin = s.BeginRates.First(t => t.Currency == CurrencyCodes.BYN).Rate;
                var bynEnd = s.EndRates.First(t => t.Currency == CurrencyCodes.BYN).Rate;
                return $"Byn {HowCurrencyChanged(bynBegin, bynEnd)}:  {bynBegin:#,0.0000} - {bynEnd:#,0.0000}";
            }
        }
        private void FillInRatesList(Saldo s)
        {
            var euroBegin = 1 / s.BeginRates.First(t => t.Currency == CurrencyCodes.EUR).Rate;
            var euroEnd = 1 / s.EndRates.First(t => t.Currency == CurrencyCodes.EUR).Rate;
            var rurBegin = s.BeginRates.First(t => t.Currency == CurrencyCodes.RUB).Rate;
            var rurEnd = s.EndRates.First(t => t.Currency == CurrencyCodes.RUB).Rate;

            Blank.RatesList = new ObservableCollection<string>
            {
             $"Изменение курсов\n\n  {HowBelarussianCurrencyChanged(s)}\n" +
             $"  Euro {HowCurrencyChanged(euroEnd, euroBegin)}:  {euroBegin:0.###} - {euroEnd:0.###} \n" +
             $"  Rur {HowCurrencyChanged(rurBegin, rurEnd)}:  {rurBegin:0.###} - {rurEnd:0.###} \n"
            };
        }

        private void CalculateForecast(Saldo s)
        {
            Blank.ForecastListIncomes = new ObservableCollection<string> { "Прогноз доходов           \n" };
            foreach (var income in s.ForecastRegularIncome.Payments)
            {
                Blank.ForecastListIncomes.Add(
                    $"{income.Amount:#,0} {income.Currency.ToString().ToLower()} {income.ArticleName}");
            }
            if (s.ForecastRegularIncome.EstimatedSum > 0)
                Blank.ForecastListIncomes.Add($"\nЕще ожидается  {s.ForecastRegularIncome.EstimatedSum:#,0} usd");
            Blank.ForecastListIncomes.Add($"\nИтого  {s.ForecastRegularIncome.TotalInUsd:#,0} usd");

            var daysInMonth = s.StartDate.AddMonths(1).AddDays(-1).Day;
            var passedDays = DateTime.Today.Year == s.StartDate.Year && DateTime.Today.Month == s.StartDate.Month
                               ? DateTime.Today.Day
                               : daysInMonth;
            Blank.ForecastListExpense = new ObservableCollection<string> { "Прогноз расходов\n" };
            var averageExpenseInUsd = (s.Expense.TotalInUsd - s.Expense.TotalForLargeInUsd) / passedDays;
            Blank.ForecastListExpense.Add(
                $"Среднедневные расходы {averageExpenseInUsd:#,0} usd ( {averageExpenseInUsd*(decimal) s.EndRates.First(c => c.Currency == CurrencyCodes.BYR).Rate:#,0} byr)");
            Blank.ForecastListExpense.Add(
                $"За {daysInMonth} дней составит: {averageExpenseInUsd*daysInMonth:#,0} usd ( {averageExpenseInUsd*(decimal) s.EndRates.First(c => c.Currency == CurrencyCodes.BYR).Rate*daysInMonth:#,0} byr)");
            Blank.ForecastListExpense.Add($" + крупные расходы:  {s.Expense.TotalForLargeInUsd:#,0} usd");
            s.ForecastExpense = averageExpenseInUsd * daysInMonth + s.Expense.TotalForLargeInUsd;
            Blank.ForecastListExpense.Add($"\nИтого расходов {s.ForecastExpense:#,0} usd ");

            Blank.ForecastListBalance = new ObservableCollection<string>
                                    {
                                      "Прогноз результата\n\n",
                                        $"Финансовый результат  {s.ForecastFinResult:#,0} usd",
                                        $" (с учетом курсовых разниц  {s.ForecastFinResult + s.ExchangeDifference:#,0} usd)\n",
                                        $"Исходящий остаток  {s.ForecastEndBalance:#,0} usd",
                                        $" (с учетом курсовых разниц  {s.ForecastEndBalance + s.ExchangeDifference:#,0} usd)"
                                    };
        }
    }
}