using System;
using System.Composition;
using System.Globalization;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils.Accounts;
using Keeper.Utils.Rates;

namespace Keeper.Utils
{
  [Export]
  public class DepositParser : PropertyChangedBase
  {
    private readonly KeeperDb _db;
    private readonly RateExtractor _rateExtractor;
    private readonly AccountTreeStraightener _accountTreeStraightener;

    [ImportingConstructor]
    public DepositParser(KeeperDb db, RateExtractor rateExtractor, AccountTreeStraightener accountTreeStraightener)
    {
      _db = db;
      _rateExtractor = rateExtractor;
      _accountTreeStraightener = accountTreeStraightener;
    }

    public DepositEvaluations Analyze(Account account)
    {
      if (account.Deposit.Bank == null)
      {
        var bankName = account.Name.Substring(0, account.Name.IndexOf(' ') - 1);
        account.Deposit.Bank = _accountTreeStraightener.Seek(bankName, _db.Accounts);
      }

      var depositEvaluations = new DepositEvaluations { DepositCore = account.Deposit };

      ExtractTraffic(depositEvaluations);
      account.Deposit.Currency = depositEvaluations.Traffic.First().Currency;
      EvaluateTraffic(depositEvaluations);
      DefineCurrentState(depositEvaluations);
      if (depositEvaluations.State != DepositStates.Закрыт) MakeForecast(depositEvaluations);
      return depositEvaluations;
    }

    /// <summary>
    /// из предположения, что обратные слэши только в датах, и даты с обеих сторон имеют пробелы
    /// </summary>
    private void ExtractInfoFromName(Deposit deposit)
    {
      var s = deposit.ParentAccount.Name;
      deposit.Bank = _accountTreeStraightener.Seek(s.Substring(0, s.IndexOf(' ')), _db.Accounts);
      var p = s.IndexOf('/');
      var n = s.IndexOf(' ', p);
      deposit.StartDate = Convert.ToDateTime(s.Substring(p - 2, n - p + 2), new CultureInfo("ru-RU"));
      p = s.IndexOf('/', n);
      n = s.IndexOf(' ', p);
      deposit.FinishDate = Convert.ToDateTime(s.Substring(p - 2, n - p + 2), new CultureInfo("ru-RU"));
      p = s.IndexOf('%', n);
      deposit.DepositRate = Convert.ToDecimal(s.Substring(n, p - n));
    }

    private void ExtractTraffic(DepositEvaluations depositEvaluations)
    {
      depositEvaluations.Traffic = (from t in _db.Transactions
                                    where t.Debet == depositEvaluations.DepositCore.ParentAccount || t.Credit == depositEvaluations.DepositCore.ParentAccount
                                    orderby t.Timestamp
                                    join r in _db.CurrencyRates on new { t.Timestamp.Date, t.Currency } equals new { r.BankDay.Date, r.Currency } into g
                                    from rate in g.DefaultIfEmpty()
                                    select new DepositTransaction{Amount = t.Amount, Timestamp = t.Timestamp, Currency = t.Currency, Comment = t.Comment,
                                                                  AmountInUsd = rate != null ? t.Amount / (decimal)rate.Rate : t.Amount,
                                                                  TransactionType = t.Operation == OperationType.Доход ? 
                                                                                                        DepositOperations.Проценты :
                                                                                                        t.Debet == depositEvaluations.DepositCore.ParentAccount ? 
                                                                                                               DepositOperations.Расход : 
                                                                                                               DepositOperations.Явнес}).ToList();
    }

    private void EvaluateTraffic(DepositEvaluations depositEvaluations)
    {
      depositEvaluations.TotalMyIns = depositEvaluations.Traffic.Where(t => t.TransactionType == DepositOperations.Явнес).Sum(t => t.Amount);
      depositEvaluations.TotalMyOuts = depositEvaluations.Traffic.Where(t => t.TransactionType == DepositOperations.Расход).Sum(t => t.Amount);
      depositEvaluations.TotalPercent = depositEvaluations.Traffic.Where(t => t.TransactionType == DepositOperations.Проценты).Sum(t => t.Amount);

      depositEvaluations.CurrentProfit = _rateExtractor.GetUsdEquivalent(depositEvaluations.CurrentBalance, depositEvaluations.DepositCore.Currency, DateTime.Today)
                              - depositEvaluations.Traffic.Where(t => t.TransactionType == DepositOperations.Явнес).Sum(t => t.AmountInUsd)
                              + depositEvaluations.Traffic.Where(t => t.TransactionType == DepositOperations.Расход).Sum(t => t.AmountInUsd); 
    }

    private void DefineCurrentState(DepositEvaluations depositEvaluations)
    {
      if (depositEvaluations.CurrentBalance == 0)
        depositEvaluations.State = DepositStates.Закрыт;
      else
        depositEvaluations.State = depositEvaluations.DepositCore.FinishDate < DateTime.Today ? DepositStates.Просрочен : DepositStates.Открыт;
    }

    private void MakeForecast(DepositEvaluations dEvaluations)
    {
      var lastProcentTransaction = dEvaluations.Traffic.LastOrDefault(t => t.TransactionType == DepositOperations.Проценты);
      var lastProcentDate = lastProcentTransaction == null ? dEvaluations.DepositCore.StartDate : lastProcentTransaction.Timestamp;

      dEvaluations.EstimatedProcents = dEvaluations.CurrentBalance * dEvaluations.DepositCore.DepositRate / 100 * (dEvaluations.DepositCore.FinishDate - lastProcentDate).Days / 365;
      dEvaluations.EstimatedProfitInUsd = dEvaluations.CurrentProfit + _rateExtractor.GetUsdEquivalent(dEvaluations.EstimatedProcents, dEvaluations.DepositCore.Currency, DateTime.Today);
    }

    public decimal GetProfitForYear(DepositEvaluations depositEvaluations, int year)
    {
      if (depositEvaluations.CurrentProfit == 0) return 0;
      int startYear = depositEvaluations.Traffic.First().Timestamp.Year;
      int finishYear = depositEvaluations.Traffic.Last().Timestamp.AddDays(-1).Year;
      if (year < startYear || year > finishYear) return 0;
      if (startYear == finishYear) return depositEvaluations.CurrentProfit;
      int allDaysCount = (depositEvaluations.Traffic.Last().Timestamp.AddDays(-1) - depositEvaluations.Traffic.First().Timestamp).Days;
      if (year == startYear)
      {
        int startYearDaysCount = (new DateTime(startYear, 12, 31) - depositEvaluations.Traffic.First().Timestamp).Days;
        return depositEvaluations.CurrentProfit * startYearDaysCount / allDaysCount;
      }
      if (year == finishYear)
      {
        int finishYearDaysCount = (depositEvaluations.Traffic.Last().Timestamp.AddDays(-1) - new DateTime(finishYear, 1, 1)).Days;
        return depositEvaluations.CurrentProfit * finishYearDaysCount / allDaysCount;
      }
      int yearDaysCount = (new DateTime(year, 12, 31) - new DateTime(year, 1, 1)).Days;
      return depositEvaluations.CurrentProfit * yearDaysCount / allDaysCount;
    }


  }
}
