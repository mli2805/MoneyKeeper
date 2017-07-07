using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Extentions;

namespace Keeper.Utils.CommonKeeper
{
    [Export]
    public class ComboboxCaterer
    {
        private readonly KeeperDb _db;

        [ImportingConstructor]
        public ComboboxCaterer(KeeperDb db)
        {
            _db = db;
        }

        public  List<Account> GetExpenseArticles()
        {
            return _db.FlattenAccounts().
               Where(account => account.Is("Все расходы") && account.Children.Count == 0).ToList();
        }

        public  List<Account> GetIncomeAndExpenseArticles()
        {
            return _db.FlattenAccounts().
               Where(account => (account.Is("Все доходы") || account.Is("Все расходы")) && account.Children.Count == 0).ToList();
        }

        public List<Account> GetExternalAccounts()
        {
            var result = _db.FlattenAccounts().
                            Where(account => account.Is("Внешние") && account.Children.Count == 0).ToList();
            result.Add(_db.SeekAccount("Банки"));
            return result;
        }

    }
}
