namespace Keeper.Controls.AccNameSelectionControl
{
    public class AccNameButtonVm
    {
        private readonly System.Action _action;
        public string Name { get; }

        public AccNameButtonVm(string name, System.Action action)
        {
            _action = action;
            Name = name;
        }

        public void Click()
        {
            _action();
        }
    }
}