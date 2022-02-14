using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class AccNameSelectorForInvestment
    {
        public static void InitializeForInvestments(this AccNameSelectorVm selector, InvestTranModel tran, ComboTreesProvider comboTreesProvider)
        {
            switch (tran.InvestOperationType)
            {
                case InvestOperationType.TopUpTrustAccount:
                    Build(selector, "Откуда", ButtonCollections.ForIncome,
                        comboTreesProvider.MyAccNamesForExpense, tran.AccountModel?.Id ?? 0, 167);
                    return;
                default:
                case InvestOperationType.WithdrawFromTrustAccount:
                    Build(selector, "Куда", ButtonCollections.ForExpense,
                        comboTreesProvider.MyAccNamesForIncome, tran.AccountModel?.Id ?? 0, 162);
                    return;
            }
        }

        private static void Build(AccNameSelectorVm selector, string controlTitle, Dictionary<string, int> frequentAccountButtonNames,
            List<AccName> availableAccNames, int activeAccountId, int defaultAccountId)
        {
            selector.ControlTitle = controlTitle;
            selector.Buttons = frequentAccountButtonNames.Select(
                button => new AccNameButtonVm(button.Key,
                    availableAccNames.FindThroughTheForestById(button.Value))).ToList();
            selector.AvailableAccNames = availableAccNames;
            selector.MyAccName = availableAccNames.FindThroughTheForestById(activeAccountId)
                                 ?? availableAccNames.FindThroughTheForestById(defaultAccountId);
        }
    }
}