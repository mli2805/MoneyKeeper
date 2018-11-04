using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace Keeper2018.TagPickingControl
{
    public class TagPickerVm : PropertyChangedBase
    {
        public ObservableCollection<AccName> Tags { get; set; } = new ObservableCollection<AccName>();
        public AccName TagInWork { get; set; }
        public AccNameSelectionControl.AccNameSelectorVm TagSelectorVm { get; set; }
        public AccName AssociatedTag { get; set; }
    }
}
