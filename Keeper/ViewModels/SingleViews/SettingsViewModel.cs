using System.Collections.ObjectModel;
using Caliburn.Micro;
using Keeper.Utils.Common;

namespace Keeper.ViewModels.SingleViews
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
