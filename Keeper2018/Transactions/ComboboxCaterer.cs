using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public class ComboboxCaterer
    {
        private readonly KeeperDb _db;

        public ComboboxCaterer(KeeperDb db)
        {
            _db = db;
        }

        public  List<Account> GetExpenseArticles()
        {
            return _db.AccountPlaneList.
               Where(account => account.Is("Все расходы") && account.Children.Count == 0).ToList();
        }

        public  List<Account> GetIncomeAndExpenseArticles()
        {
            return _db.AccountPlaneList.
               Where(account => (account.Is("Все доходы") || account.Is("Все расходы")) && account.Children.Count == 0).ToList();
        }

        public List<Account> GetExternalAccounts()
        {
            var result = _db.AccountPlaneList.
                            Where(account => account.Is("Внешние") && account.Children.Count == 0).ToList();
            result.Add(_db.SeekAccount("Банки"));
            return result;
        }

    }
}
