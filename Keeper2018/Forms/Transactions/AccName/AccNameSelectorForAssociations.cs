using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public static class AccNameSelectorForAssociations
    {
        public static void InitializeForAssociation(this AccNameSelectorVm selector, AssociationEnum associationType, int selectedId, ComboTreesProvider comboTreesProvider)
        {
            switch (associationType)
            {
                case AssociationEnum.IncomeForExternal:
                    Build(selector, "Для дохода", ButtonCollections.IncomesForExternal,
                        comboTreesProvider.GetFullBranch(185), selectedId, 0); return;
                case AssociationEnum.ExpenseForExternal:
                    Build(selector, "Для расхода", ButtonCollections.ExpensesForExternal,
                        comboTreesProvider.GetFullBranch(189), selectedId, 0); return;
                case AssociationEnum.ExternalForIncome:
                    Build(selector, "Контрагент", ButtonCollections.ExternalForIncome,
                        comboTreesProvider.GetFullBranch(157), selectedId, 0); return;
                case AssociationEnum.ExternalForExpense:
                default:
                    Build(selector, "Контрагент", ButtonCollections.ExternalForExpense,
                        comboTreesProvider.GetFullBranch(157), selectedId, 0); return;
            }
        }
        
        private static void Build(AccNameSelectorVm selector,
            string controlTitle, Dictionary<string, int> frequentAccountButtonNames,
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