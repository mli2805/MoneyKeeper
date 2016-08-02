using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Transactions;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.Rates;

namespace Keeper.Utils.CommonKeeper
{
  [Export]
  public class TransactionSetConvertor
  {
    private readonly KeeperDb _db;
    private readonly RateExtractor _rateExtractor;

    [ImportingConstructor]
    public TransactionSetConvertor(KeeperDb db, RateExtractor rateExtractor)
    {
      _db = db;
      _rateExtractor = rateExtractor;
    }

    /// <summary>
    /// работает гораздо быстрее чем ConvertTransactions
    /// но если нет курса за день операции, 
    /// не может взять предшествующий курс
    /// </summary>
    /// <param name="someSetOfTransactions"></param>
    /// <returns></returns>
    public IEnumerable<ConvertedTransaction> ConvertTransactionsQuery(IEnumerable<Transaction> someSetOfTransactions)
    {
      return from t in someSetOfTransactions
             join r in _db.CurrencyRates
               on new { t.Timestamp.Date, t.Currency } equals new { r.BankDay.Date, r.Currency } into g
             from rate in g.DefaultIfEmpty()
             select new ConvertedTransaction
             {
               Timestamp = t.Timestamp,
               Amount = t.Amount,
               Currency = t.Currency,
               Article = t.Article,
               AmountInUsd = rate != null ? t.Amount / (decimal)rate.Rate : t.Amount,
               Comment = t.Comment
             };
    }

    /// <summary>
    /// работает гораздо медленнее чем через запрос
    /// при переходе между месяцами заметна пауза, 
    /// но может взять последний введенный курс
    /// </summary>
    /// <param name="someSetOfTransactions"></param>
    /// <returns></returns>
    public IEnumerable<ConvertedTransaction> ConvertTransactions(IEnumerable<Transaction> someSetOfTransactions)
    {
      foreach (var t in someSetOfTransactions)
      {
        yield return new ConvertedTransaction
        {
          Timestamp = t.Timestamp,
          Amount = t.Amount,
          Currency = t.Currency,
          Article = t.Article,
          AmountInUsd = t.Currency != CurrencyCodes.USD ?
            t.Amount / (decimal)_rateExtractor.GetRateThisDayOrBefore(t.Currency, t.Timestamp)
            : t.Amount,
          Comment = t.Comment
        };
      }
    }
  }
}
