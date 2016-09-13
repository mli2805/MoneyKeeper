using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.DomainModel.DbTypes;
using Keeper.Utils.AccountEditing;

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

        public IEnumerable<string> ConvertTranWithTagsToFileContent()
        {
            if (_db.TransWithTags == null) yield break; // после перехода на новые транзакции можно убрать

            var orderedTrans = from tran in _db.TransWithTags
                               orderby tran.Timestamp
                               select tran;
            foreach (var tran in orderedTrans)
            {
                yield return _dbClassesInstanceDumper.Dump(tran);
            }
        }

        public IEnumerable<string> ConvertArticlesAssociationsToFileContent()
        {
            return _db.ArticlesAssociations.OrderBy(a=>a.OperationType).ThenBy(a=>a.ExternalAccount).
                          Select(articlesAssociation => _dbClassesInstanceDumper.Dump(articlesAssociation));
        }

        public IEnumerable<string> ConvertCurrencyRatesToFileContent()
        {
            return from rate in _db.CurrencyRates
                   orderby rate.BankDay
                   select _dbClassesInstanceDumper.Dump(rate);
        }

        public IEnumerable<string> ConvertOfficialRatesToFileContent()
        {
            return from rate in _db.OfficialRates
                orderby rate.Date
                select _dbClassesInstanceDumper.Dump(rate);
        }

    }
}