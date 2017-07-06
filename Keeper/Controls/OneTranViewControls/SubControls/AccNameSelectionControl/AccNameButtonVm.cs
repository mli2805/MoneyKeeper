using Keeper.DomainModel.WorkTypes;

namespace Keeper.Controls.OneTranViewControls.SubControls.AccNameSelectionControl
{
    public class AccNameButtonVm
    {
        public string Name { get; }
        public AccName AccName { get; }

        public AccNameButtonVm(string name, AccName accName)
        {
            Name = name;
            AccName = accName;
        }
    }
}