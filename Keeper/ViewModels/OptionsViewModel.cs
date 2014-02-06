using System.Composition;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  [Export]
  class OptionsViewModel : Screen
  {
    private Ini _myIni;
    private readonly IniProvider _iniProvider;

    public Ini MyIni  
    {
      get { return _myIni; }
      set
      {
        if (Equals(value, _myIni)) return;
        _myIni = value;
        NotifyOfPropertyChange(() => MyIni);
      }
    }

    [ImportingConstructor]
    public OptionsViewModel(Ini myIni, IniProvider iniProvider)
    {
      _myIni = myIni;
      _iniProvider = iniProvider;
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = "Настройки";
    }

    public void SaveChangesAndExit()
    {
      _iniProvider.WriteIni();
      TryClose();
    }

    public void CancelChangesAndExit()
    {
      TryClose();
    }
  }
}
