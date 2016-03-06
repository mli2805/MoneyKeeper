using System.Linq;

using Keeper.DomainModel;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;

namespace Keeper.Utils.CommonKeeper
{
    public class AssociationFinder
    {
        private readonly KeeperDb _db;

        public AssociationFinder(KeeperDb db)
        {
            _db = db;
        }

        public Account GetAssociation(Account account)
        {
            var association = (from a in _db.ArticlesAssociations
                               where a.ExternalAccount == account
                               select a).FirstOrDefault();
            return association == null ? null : association.AssociatedArticle;
        }

        public CurrencyCodes GetAccountLastCurrency(Account account)
        {
            var transaction = _db.Transactions.LastOrDefault(t => t.Debet == account);
            return transaction == null ? CurrencyCodes.BYR : transaction.Currency;
        }

        public Account GetBank(Account account)
        {

            return account.IsDeposit() ? account.Deposit.DepositOffer.BankAccount : null;
        }
    }
}
