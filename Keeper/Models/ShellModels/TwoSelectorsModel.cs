using System;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils.Common;

namespace Keeper.Models.ShellModels
{
  public class TwoSelectorsModel : PropertyChangedBase
  {
    public Period TranslatedPeriod
    {
      get { return _translatedPeriod; }
      set
      {
        if (Equals(value, _translatedPeriod)) return;
        _translatedPeriod = value;
        NotifyOfPropertyChange(() => TranslatedPeriod);
      }
    }

    public DateTime TranslatedDate
    {
      get { return new DayProcessor(_translatedDate).AfterThisDay(); }
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
    private Period _translatedPeriod;

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