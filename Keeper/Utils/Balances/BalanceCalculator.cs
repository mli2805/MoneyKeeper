using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;
using Keeper.Utils.Common;

namespace Keeper.Utils.Balances
{
	public sealed class Money
	{
		public decimal Amount { get; set; }
		public CurrencyCodes Currency { get; set; }
		public Money() { }

		public Money(CurrencyCodes currency, decimal amount)
		{
			Amount = amount;
			Currency = currency;
		}
		public static Money operator -(Money x)
		{
			return new Money(x.Currency, -x.Amount);
		}
		public static MoneyBag operator +(Money x, Money y)
		{
			return new MoneyBag(x) + new MoneyBag(y);
		}
		public static MoneyBag operator -(Money x, Money y)
		{
			if (x.Currency == y.Currency)
				return new MoneyBag(new Money(x.Currency, x.Amount - y.Amount));

			return new MoneyBag(x) - new MoneyBag(y);
		}
	}
	public sealed class MoneyBag : ReadOnlyCollection<Money>
	{
		public MoneyBag(IEnumerable<Money> money)
			: base(money.ToArray())
		{
		}
		public MoneyBag(params Money[] money)
			: base(money)
		{
		}

		public decimal this[CurrencyCodes index]
		{
			get
			{
				var m = this.FirstOrDefault(c => c.Currency == index);
				return m == null ? 0 : m.Amount;
			}
		}

		public bool IsZero { get { return Count == 0; } }
		public static MoneyBag operator -(MoneyBag x)
		{
			return new MoneyBag(x.Select(m => new Money(m.Currency, -m.Amount)));
		}
		public static MoneyBag operator -(MoneyBag x, MoneyBag y)
		{
			return x + (-y);
		}
		public static MoneyBag operator +(MoneyBag x, MoneyBag y)
		{
			return new MoneyBag(x
				.Concat(y)
				.GroupBy(m => m.Currency)
				.Select(Sum)
				.Where(m => m.Amount != 0));
		}
		static Money Sum(IEnumerable<Money> source)
		{
			var list = source.ToList();
			if (list.Count == 0) throw new Exception("Empty sequence");
			if (list.Count == 1) return list[0];
			if (list.Select(i => i.Currency).Distinct().Count() != 1)
				throw new Exception("Cannot sum different currencies");
			return new Money(list[0].Currency, list.Sum(i => i.Amount));
		}

		public static MoneyBag operator +(MoneyBag x, Money y)
		{
			return x + new MoneyBag(y);
		}

		public override string ToString()
		{
			return string.Join(", ", this);
		}
	}
	public static class MoneyExtensions
	{
		public static MoneyBag Sum<T>(this IEnumerable<T> source, Func<T, Money> selector)
		{
			return new MoneyBag(source.Select(selector)
			    .GroupBy(m => m.Currency)
			    .Select(mm => new Money(mm.Key, mm.Sum(m => m.Amount)))
			    .Where(m => m.Amount != 0));
		}
		
		public static MoneyBag Sum<T>(this IEnumerable<T> source, Func<T, MoneyBag> selector)
		{
//			return new MoneyBag(source.Select(selector)
//				.SelectMany(m => m)
//			    .GroupBy(m => m.Currency)
//			    .Select(mm => new Money(mm.Key, mm.Sum(m => m.Amount)))
//			    .Where(m => m.Amount != 0));

			return source.Select(selector).Aggregate(new MoneyBag(), (a, b) => a + b);
		}
	}
	public static class TransactionExtensions
	{
		public static Money Debit(this Transaction transaction)
		{
			return new Money(transaction.Currency, transaction.Amount);
		}
		public static Money Credit(this Transaction transaction)
		{
			return transaction.Operation != OperationType.Обмен
				? new Money(transaction.Currency, transaction.Amount)
				: new Money(transaction.Currency2.GetValueOrDefault(), transaction.Amount2);
		}

		public static MoneyBag Credit(this IEnumerable<Transaction> transactions, Func<Transaction, bool> predicate = null)
		{
			predicate = predicate ?? (tr => true);
			return transactions.Where(predicate).Sum(t => t.Credit());
		}
		public static MoneyBag Debit(this IEnumerable<Transaction> transactions, Func<Transaction, bool> predicate = null)
		{
			predicate = predicate ?? (tr => true);
			return transactions.Where(predicate).Sum(t => t.Debit());
		}

		public static MoneyBag Balance(this IEnumerable<Transaction> transactions, Func<Transaction, bool> predicate = null)
		{
			predicate = predicate ?? (tr => true);
			return transactions.Where(predicate).Sum(t => t.Credit() - t.Debit());
		}
		public static MoneyBag Balance(this IEnumerable<Transaction> transactions, Account balancedAccount, Period interval)
		{
			return transactions.Balance(t => interval.Contains(t.Timestamp) && t.EitherDebitOrCreditIs(balancedAccount));
		}

		public static bool DebitOrCreditIs(this Transaction transaction, Account account)
		{
			return transaction.Credit.Is(account) || transaction.Debet.Is(account);
		}
		public static bool EitherDebitOrCreditIs(this Transaction transaction, Account account)
		{
			return transaction.Credit.Is(account) != transaction.Debet.Is(account);
		}
	}
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
		/// вызов с параметром 2 февраля 2013 - вернет остаток по счету после 2 февраля 2013 
		/// </summary>
		/// <param name="balancedAccount">счет, по которому будет вычислен остаток</param>
		/// <param name="dateTime">день, после которого остаток</param>
		/// <returns></returns>
		public IEnumerable<MoneyPair> AccountBalancePairsAfterDay(Account balancedAccount, DateTime dateTime)
		{
			var period = new Period(new DateTime(0), new DayProcessor(dateTime).AfterThisDay());
			if (balancedAccount.Is("Все доходы") || balancedAccount.Is("Все расходы"))
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
