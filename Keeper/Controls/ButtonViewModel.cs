namespace Keeper.Controls
{
    public class ButtonViewModel
    {
        private readonly System.Action _action;
        public string Name { get; }

        public ButtonViewModel(string name, System.Action action)
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