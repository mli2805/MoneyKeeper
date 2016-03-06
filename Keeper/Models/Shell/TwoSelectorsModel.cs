using System;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.DomainModel.Extentions;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.Common;

namespace Keeper.Models.Shell
{
  public class TwoSelectorsModel : PropertyChangedBase
  {
    public Period TranslatedPeriod
    {
      get { return _translatedPeriod; }
      set
      {
        _translatedPeriod = value;
        NotifyOfPropertyChange(() => TranslatedPeriod);
      }
    }

    public DateTime TranslatedDate
    {
      get { return _translatedDate.GetEndOfDate(); }
      set 
      { 
        _translatedDate = value;
        NotifyOfPropertyChange(() => TranslatedDate);
      }
    }

      public bool ChangeControlTypeTranslatedEvent
      {
          get { return _changeControlTypeTranslatedEvent; }
          set
          {
              _changeControlTypeTranslatedEvent = value;
              NotifyOfPropertyChange(() => ChangeControlTypeTranslatedEvent);
          }
      }

      private bool _isPeriodMode;
    private Visibility _periodSelectControlVisibility;
    private Visibility _dateSelectControlVisibility;
    private DateTime _translatedDate;
    private Period _translatedPeriod;
    private bool _changeControlTypeTranslatedEvent;

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
   
  }
}