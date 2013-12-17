using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;

namespace Keeper.Utils
{
  [Export]
	public class BalanceCalculator
	{
	  private readonly IKeeperDb _db;

		[ImportingConstructor]
		public BalanceCalculator(IKeeperDb db)
		{
			_db = db;
		}

	  /// <summary>
		/// First way to build daily balances
		/// 
		/// This way doesn't consider excange rate differences!!!
		/// </summary>
		/// <param name="balancedAccount"></param>
		/// <param name="period"></param>
		/// <returns></returns>
		public IEnumerable<MoneyPair> AccountBalancePairs(Account balancedAccount, Period period)
		{
			var tempBalance =
			  (from t in _db.Transactions
			   where period.IsDateTimeIn(t.Timestamp) &&
				  (t.Credit.IsTheSameOrDescendantOf(balancedAccount) && !t.Debet.IsTheSameOrDescendantOf(balancedAccount) ||
				   (t.Debet.IsTheSameOrDescendantOf(balancedAccount) && !t.Credit.IsTheSameOrDescendantOf(balancedAccount)))
			   group t by t.Currency into g
			   select new MoneyPair
			   {
				   Currency = g.Key,
				   Amount = g.Sum(a => a.Amount * a.SignForAmount(balancedAccount))
			   }).
			  Concat // учесть вторую сторону обмена - приход денег в другой валюте
			  (from t in _db.Transactions
			   where t.Amount2 != 0 && period.IsDateTimeIn(t.Timestamp) &&
					 (t.Credit.IsTheSameOrDescendantOf(balancedAccount.Name) ||
												 t.Debet.IsTheSameOrDescendantOf(balancedAccount.Name))
			   group t by t.Currency2 into g
			   select new MoneyPair
			   {
				   Currency = (CurrencyCodes)g.Key,
				   Amount = g.Sum(a => a.Amount2 * a.SignForAmount(balancedAccount) * -1)
			   });

			return from b in tempBalance
				   group b by b.Currency into g
				   select new MoneyPair
				   {
					   Currency = g.Key,
					   Amount = g.Sum(a => a.Amount)
				   };
		}

    // TODO Test
		public IEnumerable<MoneyPair> ArticleBalancePairs(Account balancedAccount, Period period)
		{
			return from t in _db.Transactions
				   where t.Article != null && t.Article.IsTheSameOrDescendantOf(balancedAccount.Name) && period.IsDateTimeIn(t.Timestamp)
				   group t by t.Currency into g
				   select new MoneyPair
				   {
					   Currency = g.Key,
					   Amount = g.Sum(a => a.Amount)
				   };
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
			if (balancedAccount.IsTheSameOrDescendantOf("Все доходы") || balancedAccount.IsTheSameOrDescendantOf("Все расходы"))
				return ArticleBalancePairs(balancedAccount, period);
			else return AccountBalancePairs(balancedAccount, period);
		}

		/// <summary>
		/// вызов с параметром 2 февраля 2013 - вернет остаток по счету после 2 февраля 2013 
		/// </summary>
		/// <param name="balancedAccount">счет, по которому будет вычислен остаток</param>
		/// <param name="dateTime">день, после которого остаток</param>
		/// <returns></returns>
		public IEnumerable<MoneyPair> AccountBalancePairsAfterDay(Account balancedAccount, DateTime dateTime)
		{                                                    
			var period = new Period(new DateTime(0), new DayProcessor(dateTime).AfterThisDay());
			if (balancedAccount.IsTheSameOrDescendantOf("Все доходы") || balancedAccount.IsTheSameOrDescendantOf("Все расходы"))
				return ArticleBalancePairs(balancedAccount, period);
			else return AccountBalancePairs(balancedAccount, period);
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
