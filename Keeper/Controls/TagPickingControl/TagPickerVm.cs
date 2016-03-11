using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keeper.Controls.AccNameSelectionControl;

namespace Keeper.Controls.TagPickingControl
{
    class TagPickerVm
    {
        public List<AlreadyChosenTagVm> ListOfChosenTagsVm { get; set; }
        public AccNameSelectorVm TagSelectorVm { get; set; }
    }
}
