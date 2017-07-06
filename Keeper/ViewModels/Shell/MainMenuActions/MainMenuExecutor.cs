using System.Composition;

namespace Keeper.ViewModels.Shell.MainMenuActions
{
    [Export]
    public class MainMenuExecutor
    {
        private readonly MenuFileExecutor _menuFileExecutor;
        private readonly MenuFormsExecutor _menuFormsExecutor;
        private readonly MenuDiagramExecutor _menuDiagramExecutor;
        private readonly MenuToolsExecutor _menuToolsExecutor;
        private readonly MenuAboutExecutor _menuAboutExecutor;

        public bool IsDbChanged { get; set; }

        [ImportingConstructor]
        public MainMenuExecutor(MenuFileExecutor menuFileExecutor, MenuFormsExecutor menuFormsExecutor,
            MenuDiagramExecutor menuDiagramExecutor, MenuToolsExecutor menuToolsExecutor, MenuAboutExecutor menuAboutExecutor)
        {
            IsDbChanged = false;

            _menuFileExecutor = menuFileExecutor;
            _menuFormsExecutor = menuFormsExecutor;
            _menuDiagramExecutor = menuDiagramExecutor;
            _menuToolsExecutor = menuToolsExecutor;
            _menuAboutExecutor = menuAboutExecutor;
        }

        public void Execute(MainMenuAction action)
        {
            int range = (int)action / 100;
            switch (range)
            {
                case 1: _menuFileExecutor.Execute(action); break;
                case 2:
                    if (_menuFormsExecutor.Execute(action))
                    {
                        _menuFileExecutor.Execute(MainMenuAction.SaveDatabase);
                        IsDbChanged = true;
                    }
                    break;
                case 3: _menuDiagramExecutor.Execute(action); break;
                case 4: _menuToolsExecutor.Execute(action); break;
                case 5: _menuAboutExecutor.Execute(action); break;

                default:
                    break;
            }

        }

    }
}