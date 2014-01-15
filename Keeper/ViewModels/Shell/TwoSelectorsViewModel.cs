using System;
using System.Composition;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels.Shell
{
  [Export]
  public class TwoSelectorsViewModel : Screen
  {
    public Period TranslatedPeriod { get; set; }
    public DateTime TranslatedDate
    {
      get { return _translatedDate; }
      set 
      { 
        _translatedDate = value;
        NotifyOfPropertyChange(() => TranslatedDate);
      }
    }

    private bool _isPeriodMode;
    private Visibility _periodSelectControlVisibility;
    private Visibility _dateSelectControlVisibility;
    private DateTime _translatedDate;

    public bool IsPeriodMode
    {
      get
      {
        return _isPeriodMode;
      }
      set
      {
        _isPeriodMode= value;
        if (_isPeriodMode)
        {
          PeriodSelectControlVisibility = Visibility.Visible;
          DateSelectControlVisibility = Visibility.Collapsed;
        }
        else
        {
          PeriodSelectControlVisibility = Visibility.Collapsed;
          DateSelectControlVisibility = Visibility.Visible;
        }
      }
    }

    public Visibility PeriodSelectControlVisibility
    {
      get { return _periodSelectControlVisibility; }
      set
      {
        if (Equals(value, _periodSelectControlVisibility)) return;
        _periodSelectControlVisibility = value;
        NotifyOfPropertyChange(() => PeriodSelectControlVisibility);
      }
    }

    public Visibility DateSelectControlVisibility
    {
      get { return _dateSelectControlVisibility; }
      set
      {
        if (Equals(value, _dateSelectControlVisibility)) return;
        _dateSelectControlVisibility = value;
        NotifyOfPropertyChange(() => DateSelectControlVisibility);
      }
    }

    [ImportingConstructor]
    public TwoSelectorsViewModel()
    {
    }
  }
}
