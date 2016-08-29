using System.Collections.Generic;
using System.Composition;
using Caliburn.Micro;
using Keeper.Utils.Common;
using Keeper.ViewModels.SingleViews;

namespace Keeper.ViewModels.Shell.MainMenuActions
{
    [Export]
    public class MenuToolsExecutor
    {
        private IWindowManager WindowManager { get; set; }
        private readonly List<Screen> _launchedForms = new List<Screen>();

        [ImportingConstructor]
        public MenuToolsExecutor()
        {
            WindowManager = new WindowManager();

        }

        public bool Execute(MainMenuAction action)
        {
            switch (action)
            {
                case MainMenuAction.ShowSettings: ShowSettings(); break;
                case MainMenuAction.TempItem: TempItem(); break;
                case MainMenuAction.ShowToDoForm: ShowToDoForm(); break;
                case MainMenuAction.ShowRegularPaymentsForm: ShowRegularPaymentsForm(); break;
                default:
                    return false;
            }
            return false;
        }
        public void ShowSettings()
        {
            var settings = IoC.Get<MySettings>();
            var settingsForm = new SettingsViewModel(settings);
            _launchedForms.Add(settingsForm);
            WindowManager.ShowWindow(settingsForm);
        }

        public void TempItem()
        {
            //      ShowExpensePartingOxyPlotDiagram();

        }

        public void ShowToDoForm()
        {
            var toDoForm = new ToDoViewModel();
            _launchedForms.Add(toDoForm);
            WindowManager.ShowWindow(toDoForm);
        }

        public void ShowRegularPaymentsForm()
        {
            var regularPaymentsForm = IoC.Get<RegularPaymentsViewModel>();
            _launchedForms.Add(regularPaymentsForm);
            WindowManager.ShowWindow(regularPaymentsForm);
        }

    }
}
