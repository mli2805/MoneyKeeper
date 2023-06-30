using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class FolderSummaryViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;

        public List<string> ByCurrencies { get; set; } = new List<string>();
        public List<string> ByRevocability { get; set; } = new List<string>();

        public FolderSummaryViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Initialize(AccountItemModel accountItemModel)
        {
            DisplayName = accountItemModel.Name;
            var accountGroups = _dataModel.SeparateByRevocability(accountItemModel);
            accountGroups.Evaluate(_dataModel);
            ByRevocability = accountGroups.ToStringList();

            var calc = new TrafficOfBranchCalculator(_dataModel, accountItemModel,
                new Period(new DateTime(2001, 12, 31), DateTime.Today.AddDays(1)));
            var balance = calc.Evaluate();
            var balanceWithDetails = balance.EvaluateDetails(_dataModel, DateTime.Today.AddDays(1));
            ByCurrencies = balanceWithDetails.ToStrings().ToList();
        }

        public void Close() { TryClose(); }
    }


}
