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

    public Deposit Analyze(Account account)
    {
      var deposit = new Deposit{Account = account};
      ExtractInfoFromName(deposit);
      ExtractTraffic(deposit);
      deposit.Currency = deposit.Traffic.First().Currency;
      EvaluateTraffic(deposit);
      DefineCurrentState(deposit);
      MakeForecast(deposit);
      return deposit;
    }

    /// <summary>
    /// из предположения, что обратные слэши только в датах, и даты с обеих сторон имеют пробелы
    /// </summary>
    private void ExtractInfoFromName(Deposit deposit)
    {
      var s = deposit.Account.Name;
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

    private void ExtractTraffic(Deposit deposit)
    {
      deposit.Traffic = (from t in _db.Transactions
                         where t.Debet == deposit.Account || t.Credit == deposit.Account
                         orderby t.Timestamp
                         join r in _db.CurrencyRates on new { t.Timestamp.Date, t.Currency } equals new { r.BankDay.Date, r.Currency } into g
                         from rate in g.DefaultIfEmpty()
                         select new DepositTransaction{Amount = t.Amount, Timestamp = t.Timestamp, Currency = t.Currency, Comment = t.Comment,
                                                       AmountInUsd = rate != null ? t.Amount / (decimal)rate.Rate : t.Amount,
                                                       TransactionType = t.Operation == OperationType.Доход ? 
                                                                                           DepositOperations.Проценты : 
                                                                                           t.Debet == deposit.Account ? 
                                                                                                  DepositOperations.Расход : 
                                                                                                  DepositOperations.Явнес}).ToList();
    }

    private void EvaluateTraffic(Deposit deposit)
    {
      deposit.TotalMyIns = deposit.Traffic.Where(t=>t.TransactionType == DepositOperations.Явнес).Sum(t => t.Amount);
      deposit.TotalMyOuts = deposit.Traffic.Where(t => t.TransactionType == DepositOperations.Расход).Sum(t => t.Amount);
      deposit.TotalPercent = deposit.Traffic.Where(t => t.TransactionType == DepositOperations.Проценты).Sum(t => t.Amount);

      deposit.CurrentProfit = _rateExtractor.GetUsdEquivalent(deposit.CurrentBalance, deposit.Currency, DateTime.Today)
                              - deposit.Traffic.Where(t => t.TransactionType == DepositOperations.Явнес).Sum(t => t.AmountInUsd) 
                              + deposit.Traffic.Where(t => t.TransactionType == DepositOperations.Расход).Sum(t => t.AmountInUsd); 
    }

    private void DefineCurrentState(Deposit deposit)
    {
      if (deposit.CurrentBalance == 0)
        deposit.State = DepositStates.Закрыт;
      else
        deposit.State = deposit.FinishDate < DateTime.Today ? DepositStates.Просрочен : DepositStates.Открыт;
    }

    private void MakeForecast(Deposit deposit)
    {
      var lastProcentTransaction = deposit.Traffic.LastOrDefault(t => t.TransactionType == DepositOperations.Проценты);
      var lastProcentDate = lastProcentTransaction == null ? deposit.StartDate : lastProcentTransaction.Timestamp;

      deposit.EstimatedProcents = deposit.CurrentBalance * deposit.DepositRate / 100 * (deposit.FinishDate - lastProcentDate).Days / 365;
      deposit.EstimatedProfitInUsd = deposit.CurrentProfit + _rateExtractor.GetUsdEquivalent(deposit.EstimatedProcents, deposit.Currency, DateTime.Today);
    }
  }
}
