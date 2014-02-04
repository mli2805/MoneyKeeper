using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;
using Keeper.Utils.Common;

namespace Keeper.Utils.Balances
{
  [Export]
	public class BalanceCalculator
	{
		private readonly KeeperDb _db;

		[ImportingConstructor]
		public BalanceCalculator(KeeperDb db)
		{
			_db = db;
		}

		/// <summary>
		/// First way to build daily balances
		/// 
		/// 
		/// </summary>
		/// <param name="balancedAccount"></param>
		/// <param name="interval"></param>
		/// <returns></returns>
		public IEnumerable<MoneyPair> AccountBalancePairs(Account balancedAccount, Period interval)
		{
			var transactions = from t in _db.Transactions
			         where interval.Contains(t.Timestamp) && t.EitherDebitOrCreditIs(balancedAccount)
			         select t;
			IEnumerable<MoneyPair> moneyPairs = from t in transactions group t by t.Currency 
													into g select new MoneyPair
														{
															Currency = g.Key, Amount = g.Sum(a => a.Amount*a.SignForAmount(balancedAccount))
														};

			IEnumerable<MoneyPair> enumerable = 
				from t in _db.Transactions 
				where t.Amount2 != 0 
				  && interval.Contains(t.Timestamp) 
				  && (t.Credit.Is(balancedAccount.Name) || t.Debet.Is(balancedAccount.Name)) 
				group t by t.Currency2 into g select 
				new MoneyPair {Currency = (CurrencyCodes) g.Key, Amount = g.Sum(a => a.Amount2*a.SignForAmount(balancedAccount)*-1)};
			var tempBalance =
			  moneyPairs.
			  Concat // учесть вторую сторону обмена - приход денег в другой валюте
			  (enumerable);

			IEnumerable<MoneyPair> accountBalancePairs = from b in tempBalance group b by b.Currency into g select new MoneyPair {Currency = g.Key, Amount = g.Sum(a => a.Amount)};
			return accountBalancePairs;
		}

		public IEnumerable<MoneyPair> ArticleBalancePairs(Account balancedAccount, Period period)
		{
			return from t in _db.Transactions
				   where t.Article != null && t.Article.Is(balancedAccount.Name) && period.IsDateIn(t.Timestamp)
				   group t by t.Currency into g
				   select new MoneyPair
				   {
					   Currency = g.Key,
					   Amount = g.Sum(a => a.Amount)
				   };
		}

    public decimal ArticleTraffic(Account article, Period period)
    {
      var transactionsWithRates = from t in _db.Transactions
                                  where t.Article != null && t.Article.Is(article.Name) && period.IsDateIn(t.Timestamp)
                                  join r in _db.CurrencyRates on new {t.Timestamp.Date, t.Currency} equals new {r.BankDay.Date, r.Currency}
                                  select new { AmountInUsd = r != null ? t.Amount / (decimal)r.Rate : t.Amount };

      var am = transactionsWithRates.Sum(t => t.AmountInUsd);



      return am;
    }

		/// <summary>
		/// вызов с параметром 2 февраля 2013 - вернет остаток по счету на утро 2 февраля 2013 
		/// </summary>
		/// <param name="balancedAccount">счет, по которому будет вычислен остаток</param>
		/// <param name="dateTime">день, до которого остаток</param>
		/// <returns></returns>
		public IEnumerable<MoneyPair> AccountBalancePairsBeforeDay(Account balancedAccount, DateTime dateTime)
		{
			var period = new Period(new DateTime(0), new DayProcessor(dateTime).BeforeThisDay());
			if (balancedAccount.Is("Все доходы") || balancedAccount.Is("Все расходы"))
				return ArticleBalancePairs(balancedAccount, period);
			return AccountBalancePairs(balancedAccount, period);
		}

		/// <summary>
		/// Хреново!!! - запрашивает остаток по всем валютам, и возращает по одной переданной в качестве параметра 
		/// Иначе надо почти дублировать длинные AccountBalancePairs и ArticleBalancePairs, только с параметром валюта
		/// Если будет где-то тормозить, можно переписать
		/// </summary>
		/// <param name="account">счет, по которому будет вычислен остаток</param>
		/// <param name="period">период, за который учитываются обороты</param>
		/// <param name="currency">валюта, в которой учитываются обороты</param>
		/// <returns></returns>
		public decimal GetBalanceInCurrency(Account account, Period period, CurrencyCodes currency)
		{
			if (account == null) return 0;
			var balances = AccountBalancePairs(account, period);
			foreach (var balancePair in balances)
			{
				if (balancePair.Currency == currency) return balancePair.Amount;
			}
			return 0;
		}
	}
}
/*
private IEnumerable<ConvertedTransaction> ConvertTransactionsQuery(IEnumerable<Transaction> expenseTransactions)
	  {
      return from t in expenseTransactions
	           join r in _db.CurrencyRates
	             on new {t.Timestamp.Date, t.Currency} equals new {r.BankDay.Date, r.Currency} into g
	           from rate in g.DefaultIfEmpty()
	           select new ConvertedTransaction
	                    {
	                      Timestamp = t.Timestamp,
	                      Amount = t.Amount,
	                      Currency = t.Currency,
	                      Article = t.Article,
	                      AmountInUsd = rate != null ? t.Amount/(decimal) rate.Rate : t.Amount,
	                      Comment = t.Comment
	                    };
	  }
*/