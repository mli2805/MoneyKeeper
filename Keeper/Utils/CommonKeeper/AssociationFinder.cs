using System.Linq;
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
            return association?.AssociatedArticle;
        }

        public CurrencyCodes GetAccountLastCurrency(Account account)
        {
            var transaction = _db.TransWithTags.LastOrDefault(t => t.MyAccount.Is(account));
            return transaction?.Currency.GetValueOrDefault() ?? CurrencyCodes.BYN;
        }

        public Account GetBank(Account account)
        {

            return account.IsDeposit() ? account.Deposit.DepositOffer.BankAccount : null;
        }
    }
}
