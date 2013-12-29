using System;
using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using System.Linq;

using Keeper.DomainModel;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
	[Export]
	public class Dumper
	{
		public string Dump(HierarchyItem<Account> account)
		{
			var shiftedName = new string(' ', account.Depth * 2) + account.Item.Name;
			var parentForDump = account.Item.Parent == null ? 0 : account.Item.Parent.Id;
			return account.Item.Id + " ; " + shiftedName + " ; " + parentForDump + " ; " + account.Item.IsExpanded;
		}
		public string Dump(ArticleAssociation association)
		{
			return association.ExternalAccount + " ; " +
				   association.OperationType + " ; " +
				   association.AssociatedArticle;
		}
		public string Dump(CurrencyRate rate)
		{
			return rate.BankDay.ToString(new CultureInfo("ru-Ru")) + " ; " +
				   rate.Currency + " ; " +
				   Math.Round(rate.Rate, 4);
		}
		public string Dump(Transaction transaction)
		{
			var s = Convert.ToString(transaction.Timestamp, new CultureInfo("ru-Ru")) + " ; " + transaction.Operation + " ; " +
					transaction.Debet + " ; " + transaction.Credit + " ; " + transaction.Amount + " ; " + transaction.Currency + " ; " +
					transaction.Amount2 + " ; ";

			if (transaction.Currency2 == null || transaction.Currency2 == 0) s = s + "null";
			else s = s + transaction.Currency2;

			s = s + " ; " + transaction.Article + " ; " + transaction.Comment;
			return s;
		}
	}


	[Export(typeof(IDbEntriesToStringListsConverter))]
	public class DbEntriesToStringListsConverter : IDbEntriesToStringListsConverter
	{
		private readonly KeeperDb _db;
		readonly DbAccountsWalker mDbAccountsWalker;
		readonly Dumper mDumper;

		[ImportingConstructor]
		public DbEntriesToStringListsConverter(KeeperDb db, DbAccountsWalker dbAccountsWalker, Dumper dumper)
		{
			_db = db;
			mDbAccountsWalker = dbAccountsWalker;
			mDumper = dumper;
		}

		public IEnumerable<string> AccountsToList()
		{
			foreach (var account in mDbAccountsWalker.Walk(_db.Accounts))
			{
				yield return mDumper.Dump(account);
				if (account.Depth == 0) yield return "";
			}
		}

		public IEnumerable<string> SaveTransactions()
		{
			var orderedTransactions = from transaction in _db.Transactions
									  orderby transaction.Timestamp
									  select transaction;

			var prevTimestamp = new DateTime(2001, 1, 1);
			foreach (var transaction in orderedTransactions)
			{
				if (transaction.Timestamp <= prevTimestamp)
					transaction.Timestamp = prevTimestamp.AddMinutes(1);
				yield return mDumper.Dump(transaction);
				prevTimestamp = transaction.Timestamp;
			}
		}

		public IEnumerable<string> SaveArticlesAssociations()
		{
			return _db.ArticlesAssociations.Select(mDumper.Dump);
		}

		public IEnumerable<string> SaveCurrencyRates()
		{
			return from rate in _db.CurrencyRates
				   orderby rate.BankDay
				   select mDumper.Dump(rate);
		}

	}
}