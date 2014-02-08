using System;
using System.Collections.ObjectModel;
using System.Composition;
using System.Globalization;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils.Balances;
using Keeper.Utils.Rates;

namespace Keeper.Utils
{
  [Export]
  public class DepositParser : PropertyChangedBase
  {
    private readonly KeeperDb _db;
    private readonly RateExtractor _rateExtractor;
    private readonly BalanceCalculator _balanceCalculator;

    [ImportingConstructor]
    public DepositParser(KeeperDb db, RateExtractor rateExtractor, BalanceCalculator balanceCalculator)
    {
      _db = db;
      _rateExtractor = rateExtractor;
      _balanceCalculator = balanceCalculator;
    }

    public Deposit Analyze(Account account)
    {
      return CollectInfo(new Deposit{Account = account});
    }

    private Deposit CollectInfo(Deposit deposit)
    {
      ExtractInfoFromName(deposit);
      SelectTransactions(deposit);
      if (deposit.Transactions.Count == 0)
      {
        deposit.State = DepositStates.Пустой;
        return deposit;
      }
      deposit.MainCurrency = deposit.Transactions.First().Currency;
      Calculate(deposit);
//      if (deposit.Finish > DateTime.Today) ForecastProfit(deposit);
      return deposit;
    }

    /// <summary>
    /// из предположения, что обратные слэши только в датах, и даты с обеих сторон имеют пробелы
    /// </summary>
    private void ExtractInfoFromName(Deposit deposit)
    {
      var s = deposit.Account.Name;
      var p = s.IndexOf('/');
      var n = s.IndexOf(' ', p);
      deposit.Start = Convert.ToDateTime(s.Substring(p - 2, n - p + 2), new CultureInfo("ru-RU"));
      p = s.IndexOf('/', n);
      n = s.IndexOf(' ', p);
      deposit.Finish = Convert.ToDateTime(s.Substring(p - 2, n - p + 2), new CultureInfo("ru-RU"));
      p = s.IndexOf('%', n);
      deposit.DepositRate = Convert.ToDecimal(s.Substring(n, p - n));
    }

    private void SelectTransactions(Deposit deposit)
    {
      deposit.Transactions = (from transaction in _db.Transactions
                              where transaction.Debet == deposit.Account || transaction.Credit == deposit.Account
                      orderby transaction.Timestamp
                      select transaction).ToList();
    }

    private void Calculate(Deposit deposit)
    {
      deposit.CurrentBalance = _balanceCalculator.GetBalanceInCurrency(deposit.Account,
                                                                   new Period(new DateTime(0), DateTime.Today),
                                                                   deposit.MainCurrency);
      if (deposit.CurrentBalance == 0)
        deposit.State = DepositStates.Закрыт;
      else
        deposit.State = deposit.Finish < DateTime.Today ? DepositStates.Просрочен : DepositStates.Открыт;

      deposit.Profit = 0;
      decimal balanceAfterTransaction = 0;
      foreach (var transaction in deposit.Transactions)
      {
        var rate = transaction.Currency != CurrencyCodes.USD
                     ? _rateExtractor.GetRate(transaction.Currency, transaction.Timestamp)
                     : 1.0;

        if (transaction.Credit == deposit.Account)
        {
          if (transaction.Operation == OperationType.Перенос)
          {
            if (balanceAfterTransaction == 0)
              deposit.StartAmount = transaction.Amount;
            else
              deposit.AdditionalAmounts += transaction.Amount;
            deposit.Profit = deposit.Profit - transaction.Amount / (decimal)rate;
          }

          balanceAfterTransaction += transaction.Amount;
        }
        else
        {
          deposit.Profit = deposit.Profit + transaction.Amount / (decimal)rate;
          balanceAfterTransaction -= transaction.Amount;
        }

      }

      if (deposit.CurrentBalance != 0)
      {
        var todayRate = deposit.MainCurrency != CurrencyCodes.USD ? _rateExtractor.GetLastRate(deposit.MainCurrency) : 1.0;
        deposit.Profit += deposit.CurrentBalance / (decimal)todayRate;
      }
      deposit.Forecast = deposit.CurrentBalance * deposit.DepositRate / 100 * (deposit.Finish - deposit.Start).Days / 365;
    }
  }
}
