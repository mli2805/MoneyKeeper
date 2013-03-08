using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils;

namespace Keeper.ViewModels
{
  public class ChartPoint
  {
    public string Subject { get; set; }
    public int Amount { get; set; }

    public ChartPoint(string subject, int amount)
    {
      Subject = subject;
      Amount = amount;
    }
  }

  public class DateProcentPoint
  {
    public DateTime Date { get; set; }
    public decimal Procent { get; set; }

    public DateProcentPoint(DateTime date, decimal procent)
    {
      Date = date;
      Procent = procent;
    }
  }

  public class DepositsViewModel : Screen
  {
    private Visibility _yearsProfitVisibility;
    private Visibility _proportionsChartVisibility;

    public static IWindowManager WindowManager
    {
      get { return IoC.Get<IWindowManager>(); }
    }

    public static KeeperTxtDb Db
    {
      get { return IoC.Get<KeeperTxtDb>(); }
    }

    public List<Deposit> DepositsList { get; set; }
    public List<ChartPoint> TotalsList { get; set; }
    public List<ChartPoint> YearsList { get; set; }
    public Deposit SelectedDeposit { get; set; }
    public List<DepositViewModel> LaunchedViewModels { get; set; }

    public Style MyTitleStyle { get; set; }

    public DepositsViewModel()
    {
      MyTitleStyle = new Style();

      DepositsList = new List<Deposit>();
      foreach (var account in Db.AccountsPlaneList)
      {
        if (account.IsDescendantOf("Депозиты") && account.Children.Count == 0)
        {
          var temp = new Deposit {Account = account};
          temp.CollectInfo();
          DepositsList.Add(temp);
        }
      }
      SelectedDeposit = DepositsList[0];
      TotalBalances();
      YearsProfit();

      ProportionChartCtor();
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = "Депозиты";
    }

    public override void CanClose(Action<bool> callback)
    {
      if (LaunchedViewModels != null)
        foreach (var depositViewModel in LaunchedViewModels)
          if (depositViewModel.IsActive) depositViewModel.TryClose();
      base.CanClose(callback);
    }

    public void ShowSelectedDeposit()
    {
      if (LaunchedViewModels == null) LaunchedViewModels = new List<DepositViewModel>();
      else
      {
        var depositView = (from d in LaunchedViewModels
                           where d.Deposit.Account == SelectedDeposit.Account
                           select d).FirstOrDefault();
        if (depositView != null) depositView.TryClose();
      }
      var depositViewModel = new DepositViewModel(SelectedDeposit.Account);
      LaunchedViewModels.Add(depositViewModel);
      WindowManager.ShowWindow(depositViewModel);
    }

    public void TotalBalances()
    {
      TotalsList = new List<ChartPoint>();
      var totalBalances = new Dictionary<CurrencyCodes, decimal>();

      foreach (var deposit in DepositsList)
      {
        if (deposit.CurrentBalance == 0) continue;
        decimal total;
        if (totalBalances.TryGetValue(deposit.MainCurrency, out total))
          totalBalances[deposit.MainCurrency] = total + deposit.CurrentBalance;
        else
          totalBalances.Add(deposit.MainCurrency, deposit.CurrentBalance);
      }

      foreach (var currency in totalBalances.Keys)
      {
        if (currency == CurrencyCodes.USD)
          TotalsList.Add(
            new ChartPoint(
              String.Format("{0:#,0} {1}", totalBalances[currency], currency),
              (int) totalBalances[currency]));
        else
        {
          var inUsd = totalBalances[currency]/(decimal) Rate.GetLastRate(currency);
          TotalsList.Add(
            new ChartPoint(
              String.Format("{0:#,0} {1}", totalBalances[currency], currency),
              (int) Math.Round(inUsd)));
        }
      }
    }

    public void YearsProfit()
    {

      YearsList = new List<ChartPoint>();

      for (int i = 2002; i <= DateTime.Today.Year; i++)
      {
        decimal yearTotal = 0;
        foreach (var deposit in DepositsList)
        {
          yearTotal += deposit.GetProfitForYear(i);
        }
        if (yearTotal != 0)
          YearsList.Add(
            new ChartPoint(
              String.Format("{0}\n {1:#,0}$/мес", i, yearTotal/12),
              (int) yearTotal));
      }
    }


    public Visibility YearsProfitVisibility 
    {
      get { return _yearsProfitVisibility; }
      set
      {
        if (Equals(value, _yearsProfitVisibility)) return;
        _yearsProfitVisibility = value;
        NotifyOfPropertyChange(() => YearsProfitVisibility);
      }
    }

    public Visibility ProportionsChartVisibility
    {
      get { return _proportionsChartVisibility; }
      set
      {
        if (Equals(value, _proportionsChartVisibility)) return;
        _proportionsChartVisibility = value;
        NotifyOfPropertyChange(() => ProportionsChartVisibility);
      }
    }

    public List<DateProcentPoint> Series1 { get; set; }
    public List<DateProcentPoint> Series2 { get; set; }
    public List<DateProcentPoint> Series3 { get; set; }

    public void ProportionChartCtor()
    {
      YearsProfitVisibility = Visibility.Visible;
      ProportionsChartVisibility = Visibility.Hidden;

      var days = new Dictionary<DateTime, List<Balance.BalancePair>>();
      

      var rootDepo = Db.FindAccountInTree("Депозиты");
      var depoDates = (from t in Db.Transactions
                       where t.Debet.IsDescendantOf("Депозиты") || t.Credit.IsDescendantOf("Депозиты")
                       select t.Timestamp.Date).Distinct();
      foreach (var date in depoDates)
      {
        var allCurrencies = 
          Balance.AccountBalancePairsAfterDay(rootDepo, date).OrderByDescending(pair => pair.Currency).ToList();
        foreach (var pair in allCurrencies)  // переводим суммы в доллары, оставляя название валюты
        {
          if (pair.Currency != CurrencyCodes.USD) 
                        pair.Amount = pair.Amount / (decimal)Rate.GetRateThisDayOrBefore(pair.Currency, date);
        }
        var totalinUsd = (from p in allCurrencies select p.Amount).Sum();
        decimal totalProcents = 0;
        foreach (var pair in allCurrencies)  // переводим доллары в проценты , накопительным итогом 
                                             // пары отсортированы по валютам
        {
          totalProcents += Math.Round(pair.Amount / totalinUsd * 10000) / 100;
          pair.Amount = totalProcents;
        }

        days[date] = allCurrencies; 
      }

      Series1 = new List<DateProcentPoint>();
      Series2 = new List<DateProcentPoint>();
      Series3 = new List<DateProcentPoint>();
      foreach (var day in days)
      {
        var list = day.Value;
        foreach (var pair in list)
        {
          if (pair.Currency == CurrencyCodes.USD) Series1.Add(new DateProcentPoint(day.Key, pair.Amount));
          if (pair.Currency == CurrencyCodes.BYR) Series2.Add(new DateProcentPoint(day.Key, pair.Amount));
          if (pair.Currency == CurrencyCodes.EUR) Series3.Add(new DateProcentPoint(day.Key, pair.Amount));
        }
      }   

    }

    public CurrencyCodes CurrencyExtractor(Balance.BalancePair pair)
    {
      return pair.Currency;
    }

    public void ShowProportion()
    {
      if (YearsProfitVisibility == Visibility.Visible)
      {
        YearsProfitVisibility = Visibility.Hidden;
        ProportionsChartVisibility = Visibility.Visible;
      }
      else
      {
        ProportionsChartVisibility = Visibility.Hidden;
        YearsProfitVisibility = Visibility.Visible;
      }
    }
  }
}
