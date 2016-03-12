using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keeper.DomainModel.WorkTypes;

namespace Keeper.Controls.TagPickingControl
{
    class AlreadyChosenTagVm
    {
        public string Name => Tag.Name;
        public AccName Tag { get; set; }
    }
}
