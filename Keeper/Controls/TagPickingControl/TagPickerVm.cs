using System.Collections.Generic;
using Keeper.Controls.AccNameSelectionControl;

namespace Keeper.Controls.TagPickingControl
{
    class TagPickerVm
    {
        public List<AlreadyChosenTagVm> ListOfChosenTagsVm { get; set; } = new List<AlreadyChosenTagVm>();
        public AccNameSelectorVm TagSelectorVm { get; set; }
    }
}
