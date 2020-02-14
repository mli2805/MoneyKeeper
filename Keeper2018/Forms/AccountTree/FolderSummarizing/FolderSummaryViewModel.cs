using System;
using System.Collections.Generic;
using System.Linq;
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

            var calc = new TrafficOfBranchCalculator(_db, accountModel,
                new Period(new DateTime(2001, 12, 31), DateTime.Today.AddDays(1)));
            var balance = calc.Evaluate();
            var balanceWithDetails = balance.EvaluateDetails(_db, DateTime.Today.AddDays(1));
            ByCurrencies = balanceWithDetails.ToStrings().ToList();
        }

        public void Close() { TryClose(); }
    }


}
