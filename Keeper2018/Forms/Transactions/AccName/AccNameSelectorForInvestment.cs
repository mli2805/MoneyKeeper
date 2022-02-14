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
                case InvestOperationType.PayBaseCommission:
                case InvestOperationType.PayPurchaseFee:
                case InvestOperationType.PayWithdrawalTax:
                    Build(selector, "Откуда", ButtonCollections.InvestAccounts,
                        comboTreesProvider.AccNamesForInvestmentExpense, tran.AccountModel?.Id ?? 695);
                    return;

                case InvestOperationType.EnrollCouponOrDividends:
                    Build(selector, "Откуда", ButtonCollections.InvestAccounts,
                        comboTreesProvider.AccNamesForInvestmentIncome, tran.AccountModel?.Id ?? 696);
                    return;
                default:
                case InvestOperationType.WithdrawFromTrustAccount:
                    Build(selector, "Куда", ButtonCollections.InvestAccounts,
                        comboTreesProvider.AccNamesForInvestmentExpense, tran.AccountModel?.Id ?? 829);
                    return;
            }
        }

        private static void Build(AccNameSelectorVm selector, string controlTitle, Dictionary<string, int> frequentAccountButtonNames,
            List<AccName> availableAccNames, int activeAccountId)
        {
            selector.ControlTitle = controlTitle;
            selector.Buttons = frequentAccountButtonNames.Select(
                button => new AccNameButtonVm(button.Key,
                    availableAccNames.FindThroughTheForestById(button.Value))).ToList();
            selector.AvailableAccNames = availableAccNames;
            selector.MyAccName = availableAccNames.FindThroughTheForestById(activeAccountId);
        }
    }
}