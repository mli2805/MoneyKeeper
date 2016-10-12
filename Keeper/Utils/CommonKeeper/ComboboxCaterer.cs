using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.DomainModel.DbTypes;
using Keeper.Utils.AccountEditing;

namespace Keeper.Utils.CommonKeeper
{
    [Export]
    public class ComboboxCaterer
    {
        private readonly KeeperDb _db;
        private readonly AccountTreeStraightener _accountTreeStraightener;

        [ImportingConstructor]
        public ComboboxCaterer(KeeperDb db, AccountTreeStraightener accountTreeStraightener)
        {
            _db = db;
            _accountTreeStraightener = accountTreeStraightener;
        }

        public  List<Account> GetExpenseArticles()
        {
            return _accountTreeStraightener.Flatten(_db.Accounts).
               Where(account => account.Is("Все расходы") && account.Children.Count == 0).ToList();
        }

        public  List<Account> GetIncomeAndExpenseArticles()
        {
            return _accountTreeStraightener.Flatten(_db.Accounts).
               Where(account => (account.Is("Все доходы") || account.Is("Все расходы")) && account.Children.Count == 0).ToList();
        }

        public List<Account> GetExternalAccounts()
        {
            var result = _accountTreeStraightener.Flatten(_db.Accounts).
                            Where(account => account.Is("Внешние") && account.Children.Count == 0).ToList();
            result.Add(_accountTreeStraightener.Seek("Банки", _db.Accounts));
            return result;
        }

    }
}
