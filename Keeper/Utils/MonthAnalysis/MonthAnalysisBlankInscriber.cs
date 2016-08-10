using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.WorkTypes;
using Keeper.Models;
using Keeper.Utils.BalanceEvaluating.Ilya;
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
            Blank.BeforeList = FillListWithDateBalanceInCurrencies(s.BeginBalance.Common, s.StartDate, "�������� ������� �� ������ ������");
            Blank.BeforeListOnHands = FillListWithDateBalanceInCurrencies(s.BeginBalance.OnHands, s.StartDate, "�� �����                       ");
            Blank.BeforeListOnDeposits = FillListWithDateBalanceInCurrencies(s.BeginBalance.OnDeposits, s.StartDate, "��������");
        }

        private ObservableCollection<string> FillListWithDateBalanceInCurrencies(MoneyBagWithTotal balance, DateTime date, string caption)
        {
            var content = new List<string>();
            content.Add(caption);
            content.Add("");
            content.AddRange(MoneyBagToListOfStrings(balance.MoneyBag, date));
            content.Add("");
            content.Add($"����� {balance.TotalInUsd:#,0} usd");

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
            Blank.IncomesToHandsList = new ObservableCollection<string> { String.Format("������ �� ����  {0:#,0} usd\n", s.Incomes.OnHands.TotalInUsd) };
            foreach (var tran in s.Incomes.OnHands.Trans)
            {
                Blank.IncomesToHandsList.Add(TransactionForIncomesList(tran));
            }

            Blank.IncomesToDepositsList = new ObservableCollection<string> { String.Format("�������� �� ���������   {0:#,0} usd\n", s.Incomes.OnDeposits.TotalInUsd) };
            foreach (var tran in s.Incomes.OnDeposits.Trans)
            {
                Blank.IncomesToDepositsList.Add(TransactionForIncomesList(tran));
            }

            Blank.IncomesTotal = new ObservableCollection<string>
       { string.Format("                                                                          ����� ������  {0:#,0} usd", s.Incomes.TotalInUsd) };
        }

        private string TransactionForIncomesList(TranForAnalysis tr)
        {
            return (tr.Currency == CurrencyCodes.USD) ?
                   String.Format("{1:#,0}  {2}  {3} {4} {5}, {0:d MMM}",
                   tr.Timestamp, tr.Amount, tr.Currency.ToString().ToLower(),
                   (tr.Category.Name == "�������� �� ���������") ? "" : tr.Category.Name, tr.DepoName, tr.Comment) :
                   String.Format("{1:#,0}  {2}  (= {3:#,0} $)  {4} {5} {6}, {0:d MMM}",
                   tr.Timestamp, tr.Amount, tr.Currency.ToString().ToLower(),
                   _rateExtractor.GetUsdEquivalent(tr.Amount, tr.Currency, tr.Timestamp),
                   (tr.Category.Name == "�������� �� ���������") ? "" : tr.Category.Name, tr.DepoName, tr.Comment);
        }

        private void FillInExpenseList(Saldo s)
        {
            Blank.ExpenseList = new ObservableCollection<string> { "������� �� �����\n" };
            foreach (var dataElement in s.Expense.Categories)
            {
                Blank.ExpenseList.Add(String.Format("{0:#,0} $ - {1}", dataElement.Amount, dataElement.Category));
            }
            Blank.ExpenseList.Add(String.Format("\n����� {0:#,0} usd", s.Expense.TotalInUsd));

            Blank.LargeExpenseList = new ObservableCollection<string> { "� ��� ����� ������� ����� ����� ������\n" };
            foreach (var largeTransaction in s.Expense.LargeTransactions)
            {
                Blank.LargeExpenseList.Add(TransactionForLargeExpenseList(largeTransaction));
            }
            if (s.Expense.TotalForLargeInUsd == 0) Blank.LargeExpenseList[0] = "������� ���� � ���� ������ �� ����\n";
            else
            {
                Blank.LargeExpenseList.Add(string.Format("\n����� ������� {0:#,0} usd", s.Expense.TotalForLargeInUsd));
                Blank.LargeExpenseList.Add(String.Format("\n������� ������� {0:#,0} usd", s.Expense.TotalInUsd - s.Expense.TotalForLargeInUsd));
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
            Blank.AfterList = FillListWithDateBalanceInCurrencies(s.EndBalance.Common, s.StartDate.AddMonths(1), "��������� ������� �� ����� ������");
            Blank.AfterListOnHands = FillListWithDateBalanceInCurrencies(s.EndBalance.OnHands, s.StartDate.AddMonths(1), "�� �����                         ");
            Blank.AfterListOnDeposits = FillListWithDateBalanceInCurrencies(s.EndBalance.OnDeposits, s.StartDate.AddMonths(1), "��������");
            // ���� �� �������� ���� - �������� ������� �� ���� ���������� ���
        }

        private void FillInResultList(Saldo s)
        {
            Blank.ResultList = new ObservableCollection<string> {
        String.Format( "���������� ��������� ������ {0:#,0} - {1:#,0} = {2:#,0} usd\n",
        s.Incomes.TotalInUsd, s.Expense.TotalInUsd, s.Incomes.TotalInUsd - s.Expense.TotalInUsd)};

            Blank.ResultList.Add(String.Format("�������� ������� {4:#,0} - ({0:#,0} + {1:#,0} - {2:#,0}) = {3:#,0} usd",
                                               s.BeginBalance.Common.TotalInUsd, s.Incomes.TotalInUsd, s.Expense.TotalInUsd, s.ExchangeDifference, s.EndBalance.Common.TotalInUsd));

            Blank.ResultList.Add(String.Format("\n\n\n� ������ �������� ������ {0:#,0} - {1:#,0} + {2:#,0} = {3:#,0} usd",
                   s.Incomes.TotalInUsd, s.Expense.TotalInUsd, s.ExchangeDifference, s.EndBalance.Common.TotalInUsd - s.BeginBalance.Common.TotalInUsd));
        }

        private void FillInDepositResultList(Saldo s)
        {
            Blank.DepositResultList = new ObservableCollection<string> {
                                  String.Format("�������� ��� ���.������ {0:#,0} usd", s.DepoTraffic.ToDepo)};
            Blank.DepositResultList.Add(String.Format("�������� ��� ��������� {0:#,0} usd", s.DepoTraffic.FromDepo));
            Blank.DepositResultList.Add(String.Format("\n�������� �� ��������� (*)  {0:#,0} usd", s.Incomes.OnDeposits.TotalInUsd));
            Blank.DepositResultList.Add(String.Format("�������� �������  {0:#,0} usd", s.ExchangeDepositDifference));
            Blank.DepositResultList.Add(String.Format("\n������� � ������ �������� ������ {0:#,0} usd",
              s.Incomes.OnDeposits.TotalInUsd + s.ExchangeDepositDifference));
        }

        private string HowCurrencyChanged(double a, double b)
        {
            if (a < b) return string.Format("����");
            if (a > b) return string.Format("�����");
            return string.Format("��� ���������");
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
             $"��������� ������\n\n  {HowBelarussianCurrencyChanged(s)}\n" +
             $"  Euro {HowCurrencyChanged(euroEnd, euroBegin)}:  {euroBegin:0.###} - {euroEnd:0.###} \n" +
             $"  Rur {HowCurrencyChanged(rurBegin, rurEnd)}:  {rurBegin:0.###} - {rurEnd:0.###} \n"
            };
        }

        private void CalculateForecast(Saldo s)
        {
            Blank.ForecastListIncomes = new ObservableCollection<string> { "������� �������           \n" };
            foreach (var income in s.ForecastRegularIncome.Payments)
            {
                Blank.ForecastListIncomes.Add(String.Format("{0:#,0} {1} {2}", income.Amount, income.Currency.ToString().ToLower(), income.ArticleName));
            }
            if (s.ForecastRegularIncome.EstimatedSum > 0)
                Blank.ForecastListIncomes.Add(String.Format("\n��� ���������  {0:#,0} usd", s.ForecastRegularIncome.EstimatedSum));
            Blank.ForecastListIncomes.Add(String.Format("\n�����  {0:#,0} usd", s.ForecastRegularIncome.TotalInUsd));

            var daysInMonth = s.StartDate.AddMonths(1).AddDays(-1).Day;
            var passedDays = DateTime.Today.Year == s.StartDate.Year && DateTime.Today.Month == s.StartDate.Month
                               ? DateTime.Today.Day
                               : daysInMonth;
            Blank.ForecastListExpense = new ObservableCollection<string> { "������� ��������\n" };
            var averageExpenseInUsd = (s.Expense.TotalInUsd - s.Expense.TotalForLargeInUsd) / passedDays;
            Blank.ForecastListExpense.Add(String.Format("������������� ������� {0:#,0} usd ( {1:#,0} byr)",
                                                        averageExpenseInUsd, averageExpenseInUsd * (decimal)s.EndRates.First(c => c.Currency == CurrencyCodes.BYR).Rate));
            Blank.ForecastListExpense.Add(String.Format("�� {0} ���� ��������: {1:#,0} usd ( {2:#,0} byr)",
                                                        daysInMonth, averageExpenseInUsd * daysInMonth, averageExpenseInUsd * (decimal)s.EndRates.First(c => c.Currency == CurrencyCodes.BYR).Rate * daysInMonth));
            Blank.ForecastListExpense.Add(String.Format(" + ������� �������:  {0:#,0} usd", s.Expense.TotalForLargeInUsd));
            s.ForecastExpense = averageExpenseInUsd * daysInMonth + s.Expense.TotalForLargeInUsd;
            Blank.ForecastListExpense.Add(String.Format("\n����� �������� {0:#,0} usd ", s.ForecastExpense));

            Blank.ForecastListBalance = new ObservableCollection<string>
                                    {
                                      "������� ����������\n\n",
                                      String.Format("���������� ���������  {0:#,0} usd", s.ForecastFinResult),
                                      String.Format(" (� ������ �������� ������  {0:#,0} usd)\n", s.ForecastFinResult+s.ExchangeDifference),
                                      String.Format("��������� �������  {0:#,0} usd", s.ForecastEndBalance),
                                      String.Format(" (� ������ �������� ������  {0:#,0} usd)", s.ForecastEndBalance+s.ExchangeDifference)
                                    };
        }
    }
}