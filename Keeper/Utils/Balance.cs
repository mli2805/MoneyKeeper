using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.ViewModels;

namespace Keeper.Utils
{
	public interface IBalance
	{
		/// <summary>
		/// First way to build daily balances
		/// 
		/// This way doesn't consider excange rate differences!!!
		/// </summary>
		/// <param name="balancedAccount"></param>
		/// <param name="period"></param>
		/// <returns></returns>
		IEnumerable<Balance.BalancePair> AccountBalancePairs(Account balancedAccount, Period period);

		/// <summary>
		/// вызов с параметром 2 февраля 2013 - вернет остаток по счету на утро 2 февраля 2013 
		/// </summary>
		/// <param name="balancedAccount">счет, по которому будет вычислен остаток</param>
		/// <param name="dateTime">день, до которого остаток</param>
		/// <returns></returns>
		IEnumerable<Balance.BalancePair> AccountBalancePairsBeforeDay(Account balancedAccount, DateTime dateTime);

		/// <summary>
		/// вызов с параметром 2 февраля 2013 - вернет остаток по счету после 2 февраля 2013 
		/// </summary>
		/// <param name="balancedAccount">счет, по которому будет вычислен остаток</param>
		/// <param name="dateTime">день, после которого остаток</param>
		/// <returns></returns>
		IEnumerable<Balance.BalancePair> AccountBalancePairsAfterDay(Account balancedAccount, DateTime dateTime);

		decimal BalancePairsToUsd(IEnumerable<Balance.BalancePair> inCurrencies, DateTime dateTime);

		/// <summary>
		/// переводит остатки во всех валютах по balancedAccount после dateTime в доллары
		/// </summary>
		/// <param name="balancedAccount">счет, по которому будет вычислен остаток</param>
		/// <param name="dateTime">день, после которого остаток</param>
		/// <returns></returns>
		decimal AccountBalanceAfterDayInUsd(Account balancedAccount, DateTime dateTime);

		/// <summary>
		/// Хреново!!! - запрашивает остаток по всем валютам, и возращает по одной переданной в качестве параметра 
		/// Иначе надо почти дублировать длинные AccountBalancePairs и ArticleBalancePairs, только с параметром валюта
		/// Если будет где-то тормозить, можно переписать
		/// </summary>
		/// <param name="account">счет, по которому будет вычислен остаток</param>
		/// <param name="period">период, за который учитываются обороты</param>
		/// <param name="currency">валюта, в которой учитываются обороты</param>
		/// <returns></returns>
		decimal GetBalanceInCurrency(Account account, Period period, CurrencyCodes currency);

		/// <summary>
		/// Функция нужна только заполнения для 2-й рамки на ShellView
		/// Расчитываются остатки по счету и его потомкам 1-го поколения
		/// </summary>
		decimal CountBalances(Account selectedAccount, Period period, ObservableCollection<string> balanceList);

		List<string> CalculateDayResults(DateTime dt);
		string EndDayBalances(DateTime dt);
	}

	[Export(typeof(IBalance))]
	public class Balance : IBalance
	{
		public IKeeperDb Db { get; private set; }
		public IRate Rate { get; private set; }
//		private static readonly IRate Rate = IoC.Get<IRate>();
//		public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

		[ImportingConstructor]
		public Balance(IKeeperDb db, IRate rate)
		{
			Db = db;
			Rate = rate;
		}

		public class BalancePair : IComparable
		{
			public CurrencyCodes Currency { get; set; }
			public decimal Amount { get; set; }

			public new string ToString()
			{
				return String.Format("{0:#,0} {1}", Amount, Currency.ToString().ToLower());
			}

			public int CompareTo(object obj)
			{
				return Currency.CompareTo(((BalancePair)obj).Currency);
			}
		}

		class BalanceTrio
		{
			public Account MyAccount;
			public decimal Amount;
			public CurrencyCodes Currency;

			public new string ToString()
			{
				return String.Format("{0}  {1:#,0} {2}", MyAccount.Name, Amount, Currency.ToString().ToLower());
			}
		}

		/// <summary>
		/// First way to build daily balances
		/// 
		/// This way doesn't consider excange rate differences!!!
		/// </summary>
		/// <param name="balancedAccount"></param>
		/// <param name="period"></param>
		/// <returns></returns>
		public IEnumerable<BalancePair> AccountBalancePairs(Account balancedAccount, Period period)
		{
			var tempBalance =
			  (from t in Db.Transactions
			   where period.IsDateTimeIn(t.Timestamp) &&
				  (t.Credit.IsTheSameOrDescendantOf(balancedAccount) && !t.Debet.IsTheSameOrDescendantOf(balancedAccount) ||
				   (t.Debet.IsTheSameOrDescendantOf(balancedAccount) && !t.Credit.IsTheSameOrDescendantOf(balancedAccount)))
			   group t by t.Currency into g
			   select new BalancePair
			   {
				   Currency = g.Key,
				   Amount = g.Sum(a => a.Amount * a.SignForAmount(balancedAccount))
			   }).
			  Concat // учесть вторую сторону обмена - приход денег в другой валюте
			  (from t in Db.Transactions
			   where t.Amount2 != 0 && period.IsDateTimeIn(t.Timestamp) &&
					 (t.Credit.IsTheSameOrDescendantOf(balancedAccount.Name) ||
												 t.Debet.IsTheSameOrDescendantOf(balancedAccount.Name))
			   group t by t.Currency2 into g
			   select new BalancePair
			   {
				   Currency = (CurrencyCodes)g.Key,
				   Amount = g.Sum(a => a.Amount2 * a.SignForAmount(balancedAccount) * -1)
			   });

			return from b in tempBalance
				   group b by b.Currency into g
				   select new BalancePair
				   {
					   Currency = g.Key,
					   Amount = g.Sum(a => a.Amount)
				   };
		}

		private IEnumerable<BalancePair> ArticleBalancePairs(Account balancedAccount, Period period)
		{
			return from t in Db.Transactions
				   where t.Article != null && t.Article.IsTheSameOrDescendantOf(balancedAccount.Name) && period.IsDateTimeIn(t.Timestamp)
				   group t by t.Currency into g
				   select new BalancePair
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
		public IEnumerable<BalancePair> AccountBalancePairsBeforeDay(Account balancedAccount, DateTime dateTime)
		{
			// выделение даты без времени и минус минута
			var period = new Period(new DateTime(0), dateTime.Date.AddMinutes(-1));
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
		public IEnumerable<BalancePair> AccountBalancePairsAfterDay(Account balancedAccount, DateTime dateTime)
		{                                                    // выделение даты без времени плюс день и минус минута
			var period = new Period(new DateTime(0), dateTime.Date.AddDays(1).AddMinutes(-1));
			if (balancedAccount.IsTheSameOrDescendantOf("Все доходы") || balancedAccount.IsTheSameOrDescendantOf("Все расходы"))
				return ArticleBalancePairs(balancedAccount, period);
			else return AccountBalancePairs(balancedAccount, period);
		}

		public decimal BalancePairsToUsd(IEnumerable<BalancePair> inCurrencies, DateTime dateTime)
		{
			decimal result = 0;
			foreach (var balancePair in inCurrencies)
			{
				if (balancePair.Currency == CurrencyCodes.USD) 
					result += balancePair.Amount;
				else
					result += balancePair.Amount / (decimal)Rate.GetRateThisDayOrBefore(balancePair.Currency, dateTime);
			}
			return result;
		}

		/// <summary>
		/// переводит остатки во всех валютах по balancedAccount после dateTime в доллары
		/// </summary>
		/// <param name="balancedAccount">счет, по которому будет вычислен остаток</param>
		/// <param name="dateTime">день, после которого остаток</param>
		/// <returns></returns>
		public decimal AccountBalanceAfterDayInUsd(Account balancedAccount, DateTime dateTime)
		{
			var inCurrencies = AccountBalancePairsAfterDay(balancedAccount, dateTime);
			var result = BalancePairsToUsd(inCurrencies, dateTime);
			return Math.Round(result * 100) / 100;
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

		#region для заполнения для 2-й рамки на ShellView

		private List<string> OneBalance(Account balancedAccount, Period period, out decimal totalInUsd)
		{
			var balance = new List<string>();
			totalInUsd = 0;
			if (balancedAccount == null) return balance;

			bool kind = balancedAccount.IsTheSameOrDescendantOf("Все доходы") || balancedAccount.IsTheSameOrDescendantOf("Все расходы");
			var balancePairs = kind ? ArticleBalancePairs(balancedAccount, period) : AccountBalancePairs(balancedAccount, period);

			foreach (var item in balancePairs)
			{
				if (item.Amount != 0) balance.Add(String.Format("{0:#,#} {1}", item.Amount, item.Currency));
				totalInUsd += Rate.GetUsdEquivalent(item.Amount, item.Currency, period.GetFinish());
			}

			return balance;
		}

		/// <summary>
		/// Функция нужна только заполнения для 2-й рамки на ShellView
		/// Расчитываются остатки по счету и его потомкам 1-го поколения
		/// </summary>
		public decimal CountBalances(Account selectedAccount, Period period, ObservableCollection<string> balanceList)
		{
			balanceList.Clear();
			if (selectedAccount == null) return 0;

			decimal inUsd;
			var b = OneBalance(selectedAccount, period, out inUsd);
			foreach (var st in b)
				balanceList.Add(st);

			foreach (var child in selectedAccount.Children)
			{
				decimal temp;
				b = OneBalance(child, period, out temp);
				if (b.Count > 0) balanceList.Add("         " + child.Name);
				foreach (var st in b)
					balanceList.Add("    " + st);
			}

			return inUsd;
		}
		#endregion

		#region для заполнения окошек на TransactionsView

		public List<string> CalculateDayResults(DateTime dt)
		{
			var dayResults = new List<string> { String.Format("                              {0:dd MMMM yyyy}", dt.Date) };

			var incomes = from t in Db.Transactions
						  where t.Operation == OperationType.Доход && t.Timestamp.Date == dt.Date
						  group t by new
									   {
										   t.Credit,
										   t.Currency
									   }
							  into g
							  select new BalanceTrio
							  {
								  MyAccount = g.Key.Credit,
								  Currency = g.Key.Currency,
								  Amount = g.Sum(a => a.Amount)
							  };

			if (incomes.Any()) dayResults.Add("  Доходы");
			foreach (var balanceTrio in incomes)
			{
				dayResults.Add(balanceTrio.ToString());
			}

			var expense = from t in Db.Transactions
						  where t.Operation == OperationType.Расход && t.Timestamp.Date == dt.Date
						  group t by new
						  {
							  t.Debet,
							  t.Currency
						  }
							  into g
							  select new BalanceTrio
									   {
										   MyAccount = g.Key.Debet,
										   Currency = g.Key.Currency,
										   Amount = g.Sum(a => a.Amount)
									   };

			if (dayResults.Count > 0) dayResults.Add("");
			if (expense.Any()) dayResults.Add("  Расходы");
			foreach (var balanceTrio in expense)
			{
				dayResults.Add(balanceTrio.ToString());
			}

			return dayResults;
		}

		public string EndDayBalances(DateTime dt)
		{
			var period = new Period(new DateTime(0), dt.Date.AddDays(1).AddMinutes(-1));
			var result = String.Format(" На конец {0:dd MMMM yyyy} :   ", dt.Date);

			var depo = (from a in Db.AccountsPlaneList
						where a.Name == "Депозиты"
						select a).First();
			var calculatedAccounts = new List<Account>(UsefulLists.MyAccountsForShopping);
			calculatedAccounts.Add(depo);
			foreach (var account in calculatedAccounts)
			{
				var pairs = AccountBalancePairs(account, period).ToList();
				foreach (var balancePair in pairs.ToArray())
					if (balancePair.Amount == 0) pairs.Remove(balancePair);
				if (pairs.Any())
					result = result + String.Format("   {0}  {1}", account.Name, pairs[0].ToString());
				if (pairs.Count() > 1)
					for (var i = 1; i < pairs.Count(); i++)
						result = result + String.Format(" + {0}", pairs[i].ToString());
			}

			return result;
		}

		#endregion
	}
}
