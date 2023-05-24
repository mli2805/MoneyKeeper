using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public partial class AccNameSelector
    {
        private readonly KeeperDataModel _dataModel;
        private readonly ComboTreesProvider _comboTreesProvider;

        public AccNameSelector(KeeperDataModel dataModel, ComboTreesProvider comboTreesProvider)
        {
            _dataModel = dataModel;
            _comboTreesProvider = comboTreesProvider;
        }

        private AccNameSelectorVm Build(string controlTitle, Dictionary<string, int> frequentAccountButtonNames,
            List<AccName> availableAccNames, int activeAccountId)
        {
            return new AccNameSelectorVm
            {
                ControlTitle = controlTitle,
                Buttons = frequentAccountButtonNames == null 
                    ? new List<AccNameButtonVm>() 
                    : frequentAccountButtonNames.Select(
                        button => new AccNameButtonVm(button.Key,
                            availableAccNames.FindThroughTheForestById(button.Value))).ToList(),
                AvailableAccNames = availableAccNames,
                MyAccName = availableAccNames.FindThroughTheForestById(activeAccountId),
            };
        }

    }
}
