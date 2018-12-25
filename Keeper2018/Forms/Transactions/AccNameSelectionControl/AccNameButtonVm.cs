namespace Keeper2018
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