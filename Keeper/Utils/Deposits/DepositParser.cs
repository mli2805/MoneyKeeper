using System;
using System.Collections.ObjectModel;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils.Accounts;
using Keeper.Utils.Balances;
using Keeper.Utils.Deposits;
using Keeper.Utils.Rates;

namespace Keeper.Utils
{
  [Export]
  public class DepositParser : PropertyChangedBase
  {
    private readonly KeeperDb _db;
    private readonly RateExtractor _rateExtractor;
    private readonly AccountTreeStraightener _accountTreeStraightener;
    private readonly DepositEvaluator _depositEvaluator;

    [ImportingConstructor]
    public DepositParser(KeeperDb db, RateExtractor rateExtractor, AccountTreeStraightener accountTreeStraightener, DepositEvaluator depositEvaluator)
    {
      _db = db;
      _rateExtractor = rateExtractor;
      _accountTreeStraightener = accountTreeStraightener;
      _depositEvaluator = depositEvaluator;
    }

    public Deposit Analyze(Account account)
    {
      if (account.Deposit == null)
      {
        account.Deposit = new Deposit { ParentAccount = account };
        ExtractInfoFromName(account);
      }

      account.Deposit.Evaluations = new DepositEvaluations();

      ExtractTraffic(account);

      account.Deposit.Currency = account.Deposit.Evaluations.Traffic.First().Currency;

      EvaluateTraffic(account);
      DefineCurrentState(account);
      if (account.Deposit.Evaluations.State != DepositStates.Закрыт) MakeForecast(account);
      return account.Deposit;
    }

    /// <summary>
    /// из предположения, что обратные слэши только в датах, и даты с обеих сторон имеют пробелы
    /// </summary>
    private void ExtractInfoFromName(Account account)
    {
      var s = account.Name;
      var n = s.IndexOf(' ');
      var bankName = s.Substring(0, n);
      var banks = _accountTreeStraightener.Seek("Банки", _db.Accounts);
      foreach (var bank in banks.Children)
      {
        if (bank.Name.Substring(0, 3) != bankName.Substring(0, 3)) continue;
        account.Deposit.Bank = bank;
        break;
      }
      if (account.Deposit.Bank == null) MessageBox.Show(bankName);

      s = s.Substring(n + 1);
      var p = s.IndexOf('/');
      account.Deposit.Title = s.Substring(0, p - 2);

      n = s.IndexOf(' ', p);
      account.Deposit.StartDate = Convert.ToDateTime(s.Substring(p - 2, n - p + 2), new CultureInfo("ru-RU"));
      p = s.IndexOf('/', n);
      n = s.IndexOf(' ', p);
      account.Deposit.FinishDate = Convert.ToDateTime(s.Substring(p - 2, n - p + 2), new CultureInfo("ru-RU"));
      p = s.IndexOf('%', n);
      account.Deposit.DepositRateLines = new ObservableCollection<DepositRateLine> 
         { new DepositRateLine{ AmountFrom = 0, AmountTo = 999999999999, DateFrom = account.Deposit.StartDate, Rate = Convert.ToDecimal(s.Substring(n, p - n))} };
    }

    private void ExtractTraffic(Account account)
    {
      account.Deposit.Evaluations.Traffic = (from t in _db.Transactions
                                             where t.Debet == account || t.Credit == account
                                             orderby t.Timestamp
                                             join r in _db.CurrencyRates on new { t.Timestamp.Date, t.Currency } equals new { r.BankDay.Date, r.Currency } into g
                                             from rate in g.DefaultIfEmpty()
                                             select new DepositTransaction
                                             {
                                               Amount = t.Amount,
                                               Timestamp = t.Timestamp,
                                               Currency = t.Currency,
                                               Comment = t.Comment,
                                               AmountInUsd = rate != null ? t.Amount / (decimal)rate.Rate : t.Amount,
                                               TransactionType = t.Operation == OperationType.Доход ?
                                                                                     DepositOperations.Проценты : t.Debet == account ?
                                                                                            DepositOperations.Расход :
                                                                                            DepositOperations.Явнес
                                             }).ToList();
    }

    private void EvaluateTraffic(Account account)
    {
      account.Deposit.Evaluations.TotalMyIns = account.Deposit.Evaluations.Traffic.Where(t => t.TransactionType == DepositOperations.Явнес).Sum(t => t.Amount);
      account.Deposit.Evaluations.TotalMyOuts = account.Deposit.Evaluations.Traffic.Where(t => t.TransactionType == DepositOperations.Расход).Sum(t => t.Amount);
      account.Deposit.Evaluations.TotalPercent = account.Deposit.Evaluations.Traffic.Where(t => t.TransactionType == DepositOperations.Проценты).Sum(t => t.Amount);

      account.Deposit.Evaluations.CurrentProfit = _rateExtractor.GetUsdEquivalent(account.Deposit.Evaluations.CurrentBalance, account.Deposit.Currency, DateTime.Today)
                              - account.Deposit.Evaluations.Traffic.Where(t => t.TransactionType == DepositOperations.Явнес).Sum(t => t.AmountInUsd)
                              + account.Deposit.Evaluations.Traffic.Where(t => t.TransactionType == DepositOperations.Расход).Sum(t => t.AmountInUsd);
    }

    private void DefineCurrentState(Account account)
    {
      if (account.Deposit.Evaluations.CurrentBalance == 0)
        account.Deposit.Evaluations.State = DepositStates.Закрыт;
      else
        account.Deposit.Evaluations.State = account.Deposit.FinishDate < DateTime.Today ? DepositStates.Просрочен : DepositStates.Открыт;
    }

    private void MakeForecast(Account account)
    {
      var lastProcentTransaction = account.Deposit.Evaluations.Traffic.LastOrDefault(t => t.TransactionType == DepositOperations.Проценты);
      var lastProcentDate = lastProcentTransaction == null ? account.Deposit.StartDate : lastProcentTransaction.Timestamp;

      account.Deposit.Evaluations.EstimatedProcents = ProcentEvaluationNew(account, lastProcentDate);
      account.Deposit.Evaluations.EstimatedProfitInUsd = account.Deposit.Evaluations.CurrentProfit + _rateExtractor.GetUsdEquivalent(account.Deposit.Evaluations.EstimatedProcents, account.Deposit.Currency, DateTime.Today);
    }

//       старый метод расчета не учитывал изменение ежедневных остатков и изменение ставок с течением времени и в зависимости от величины остатка
//    private decimal ProcentEvaluation(Account account, DateTime lastProcentDate)
//    {
//      return account.Deposit.Evaluations.CurrentBalance * account.Deposit.DepositRate / 100 *
//             (account.Deposit.FinishDate - lastProcentDate).Days / 365;
//    }

    private decimal ProcentEvaluationNew(Account account, DateTime lastProcentDate)
    {
      // новый метод
      if (account.Deposit.DepositRateLines != null)
        return _depositEvaluator.ProcentsForPeriod(account, new Period(lastProcentDate, account.Deposit.FinishDate));

      MessageBox.Show("Не заведена таблица процентных ставок!");
      return 0;
    }

    public decimal GetProfitForYear(Deposit deposit, int year)
    {
      if (deposit.Evaluations.CurrentProfit == 0) return 0;
      int startYear = deposit.Evaluations.Traffic.First().Timestamp.Year;
      int finishYear = deposit.Evaluations.Traffic.Last().Timestamp.AddDays(-1).Year;
      if (year < startYear || year > finishYear) return 0;
      if (startYear == finishYear) return deposit.Evaluations.CurrentProfit;
      int allDaysCount = (deposit.Evaluations.Traffic.Last().Timestamp.AddDays(-1) - deposit.Evaluations.Traffic.First().Timestamp).Days;
      if (year == startYear)
      {
        int startYearDaysCount = (new DateTime(startYear, 12, 31) - deposit.Evaluations.Traffic.First().Timestamp).Days;
        return deposit.Evaluations.CurrentProfit * startYearDaysCount / allDaysCount;
      }
      if (year == finishYear)
      {
        int finishYearDaysCount = (deposit.Evaluations.Traffic.Last().Timestamp.AddDays(-1) - new DateTime(finishYear, 1, 1)).Days;
        return deposit.Evaluations.CurrentProfit * finishYearDaysCount / allDaysCount;
      }
      int yearDaysCount = (new DateTime(year, 12, 31) - new DateTime(year, 1, 1)).Days;
      return deposit.Evaluations.CurrentProfit * yearDaysCount / allDaysCount;
    }


  }
}
