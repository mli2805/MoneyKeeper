using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;

using Keeper.DomainModel;
using Keeper.Utils.Accounts;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
    [Export(typeof(IDbEntriesToStringListsConverter))]
    public class DbEntriesToFileContentConverter : IDbEntriesToStringListsConverter
    {
        private readonly KeeperDb _db;
        readonly AccountTreeStraightener _accountTreeStraightener;
        readonly DbClassesInstanceDumper _dbClassesInstanceDumper;

        [ImportingConstructor]
        public DbEntriesToFileContentConverter(KeeperDb db, AccountTreeStraightener accountTreeStraightener, DbClassesInstanceDumper dbClassesInstanceDumper)
        {
            _db = db;
            _accountTreeStraightener = accountTreeStraightener;
            _dbClassesInstanceDumper = dbClassesInstanceDumper;
        }

        public IEnumerable<string> ConvertAccountsToFileContent()
        {
            foreach (var account in _accountTreeStraightener.FlattenWithLevels(_db.Accounts))
            {
                yield return _dbClassesInstanceDumper.Dump(account);
                if (account.Level == 0) yield return "";
            }
        }

        public IEnumerable<string> ConvertDepositsToFileContent()
        {
            foreach (var account in _accountTreeStraightener.FlattenWithLevels(_db.Accounts))
            {
                if (account.Item.Deposit == null) continue;
                yield return _dbClassesInstanceDumper.Dump(account.Item.Deposit);
            }
        }

        public IEnumerable<string> ConvertBankDepositOffersRatesToFileContent()
        {
            return from bankDepositOffer in _db.BankDepositOffers from rateLine in bankDepositOffer.RateLines select _dbClassesInstanceDumper.Dump(rateLine, bankDepositOffer.Id);
        }

        public IEnumerable<string> ConvertBankDepositOffersToFileContent()
        {
            return _db.BankDepositOffers.Select(bankDepositOffer => _dbClassesInstanceDumper.Dump(bankDepositOffer));
        }

        public IEnumerable<string> ConvertTransactionsToFileContent()
        {
            var orderedTransactions = from transaction in _db.Transactions
                                      orderby transaction.Timestamp
                                      select transaction;

            var prevTimestamp = new DateTime(2001, 1, 1);
            foreach (var transaction in orderedTransactions)
            {
                if (transaction.Timestamp <= prevTimestamp)
                    transaction.Timestamp = prevTimestamp.AddMinutes(1);
                yield return _dbClassesInstanceDumper.Dump(transaction);
                prevTimestamp = transaction.Timestamp;
            }
        }

        public IEnumerable<string> ConvertArticlesAssociationsToFileContent()
        {
            //			return _db.ArticlesAssociations.Select(_mDbClassesInstanceDumper.Dump);
            return _db.ArticlesAssociations.Select(articlesAssociation => _dbClassesInstanceDumper.Dump(articlesAssociation));
        }

        public IEnumerable<string> ConvertCurrencyRatesToFileContent()
        {
            return from rate in _db.CurrencyRates
                   orderby rate.BankDay
                   select _dbClassesInstanceDumper.Dump(rate);
        }

    }
}