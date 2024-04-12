using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public partial class AccNameSelector
    {
        public AccNameSelectorVm InitializeForAssociation( 
            AssociationEnum associationType, int selectedId)
        {
            switch (associationType)
            {
                case AssociationEnum.IncomeForExternal:
                    return Build("Категория для дохода", 
                        _dataModel.ButtonCollections.First(c => c.Id == 10).ToButtonsDictionary(),
                        _comboTreesProvider.GetFullBranch(185), selectedId); 
                case AssociationEnum.ExpenseForExternal:
                    return Build("Категория для расхода", 
                        _dataModel.ButtonCollections.First(c => c.Id == 11).ToButtonsDictionary(),
                        _comboTreesProvider.GetFullBranch(189), selectedId); 
                case AssociationEnum.ExternalForIncome:
                    return Build("Контрагент", 
                        _dataModel.ButtonCollections.First(c => c.Id == 8).ToButtonsDictionary(),
                        _comboTreesProvider.GetFullBranch(157), selectedId);
                case AssociationEnum.ExternalForExpense:
                default:
                    return Build("Контрагент", 
                        _dataModel.ButtonCollections.First(c => c.Id == 9).ToButtonsDictionary(),
                        _comboTreesProvider.GetFullBranch(157), selectedId); 
            }
        }

        public AccNameSelectorVm ForAssociatedTag(int selectedTagId)
        {
            return Build("Связанный тэг",
                new Dictionary<string, int>(),
                _comboTreesProvider.AdditionalTags, selectedTagId);
        }
    }
}