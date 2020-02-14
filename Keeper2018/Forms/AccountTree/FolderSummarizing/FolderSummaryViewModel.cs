using System.Collections.Generic;
using Caliburn.Micro;

namespace Keeper2018
{
    public class FolderSummaryViewModel : Screen
    {
        private readonly KeeperDb _db;

        public List<string> ByCurrencies { get; set; } = new List<string>();
        public List<string> ByRevocability { get; set; } = new List<string>();

        public FolderSummaryViewModel(KeeperDb db)
        {
            _db = db;
        }

        public void Initialize(AccountModel accountModel)
        {
            DisplayName = accountModel.Name;
            var accountGroups = _db.SeparateByRevocability(accountModel);
            accountGroups.Evaluate(_db);

            ByRevocability = accountGroups.ToStringList();
        }

        public void Close() { TryClose(); }
    }


}
