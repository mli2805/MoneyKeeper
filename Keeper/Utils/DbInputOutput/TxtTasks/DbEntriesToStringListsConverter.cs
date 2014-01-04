using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;

using Keeper.DomainModel;
using Keeper.Utils.Accounts;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
  [Export(typeof(IDbEntriesToStringListsConverter))]
	public class DbEntriesToStringListsConverter : IDbEntriesToStringListsConverter
	{
		private readonly KeeperDb _db;
		readonly AccountTreeStraightener mAccountTreeStraightener;
		readonly DbClassesInstanceDumper _mDbClassesInstanceDumper;

		[ImportingConstructor]
		public DbEntriesToStringListsConverter(KeeperDb db, AccountTreeStraightener accountTreeStraightener, DbClassesInstanceDumper dbClassesInstanceDumper)
		{
			_db = db;
			mAccountTreeStraightener = accountTreeStraightener;
			_mDbClassesInstanceDumper = dbClassesInstanceDumper;
		}

		public IEnumerable<string> AccountsToList()
		{
			foreach (var account in mAccountTreeStraightener.FlattenWithLevels(_db.Accounts))
			{
				yield return _mDbClassesInstanceDumper.Dump(account);
				if (account.Level == 0) yield return "";
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
				yield return _mDbClassesInstanceDumper.Dump(transaction);
				prevTimestamp = transaction.Timestamp;
			}
		}

		public IEnumerable<string> SaveArticlesAssociations()
		{
			return _db.ArticlesAssociations.Select(_mDbClassesInstanceDumper.Dump);
		}

		public IEnumerable<string> SaveCurrencyRates()
		{
			return from rate in _db.CurrencyRates
				   orderby rate.BankDay
				   select _mDbClassesInstanceDumper.Dump(rate);
		}

	}
}