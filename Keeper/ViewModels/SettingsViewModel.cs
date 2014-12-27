using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using Keeper.Utils.Common;

namespace Keeper.ViewModels
{

    public class SettingsViewModel : Screen
    {
        public ObservableCollection<OneSetting> Rows { get; set; }

        public SettingsViewModel(MySettings mySettings)
        {
            Rows = new ObservableCollection<OneSetting>();
            foreach (var oneSetting in mySettings)
            {
                Rows.Add(oneSetting);
            }
        }
    }
}
