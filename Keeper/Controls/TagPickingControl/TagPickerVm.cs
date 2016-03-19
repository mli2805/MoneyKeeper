using System.Collections.ObjectModel;
using Keeper.Controls.AccNameSelectionControl;
using Keeper.DomainModel.WorkTypes;

namespace Keeper.Controls.TagPickingControl
{
    class TagPickerVm
    {
        public ObservableCollection<AccName> Tags { get; set; } = new ObservableCollection<AccName>();
        public AccNameSelectorVm TagSelectorVm { get; set; }
    }
}
