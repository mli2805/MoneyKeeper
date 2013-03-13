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

      DepoCurrenciesProportionChartCtor();
      CashDepoProportionChartCtor();
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


    public List<DateProcentPoint> Series1 { get; set; }
    public List<DateProcentPoint> Series2 { get; set; }
    public List<DateProcentPoint> Series3 { get; set; }

    public void DepoCurrenciesProportionChartCtor()
    {
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

    public List<DateProcentPoint> CashSeries { get; set; }
    public List<DateProcentPoint> DepoSeries { get; set; }

//    public void CashDepoProportionChartCtor()
//    {
//      CashSeries = new List<DateProcentPoint>();
//      DepoSeries = new List<DateProcentPoint>();
//      var rootCashAccount = Db.FindAccountInTree("На руках");
//      var rootDepoAccount = Db.FindAccountInTree("Депозиты");
//      for (var dt = new DateTime(2002, 1, 1); dt <= DateTime.Today; dt = dt.AddDays(1))
//      {
//        var cashInUsd = Balance.AccountBalanceAfterDayInUsd(rootCashAccount, dt);
//        var depoInUsd = Balance.AccountBalanceAfterDayInUsd(rootDepoAccount, dt);
//        CashSeries.Add(new DateProcentPoint(dt, Math.Round(cashInUsd / (cashInUsd + depoInUsd) * 100)));
//        DepoSeries.Add(new DateProcentPoint(dt, 100));
//      }
//    }

    public void CashDepoProportionChartCtor()
    {
      CashSeries = new List<DateProcentPoint>();
      DepoSeries = new List<DateProcentPoint>();
      var rootCashAccount = Db.FindAccountInTree("На руках");
      var rootDepoAccount = Db.FindAccountInTree("Депозиты");

      var period = new Period(new DateTime(2002, 1, 1), DateTime.Today);
      var cashBalances = Balance.AccountBalancesForPeriodInUsd(rootCashAccount, period);
      var depoBalances = Balance.AccountBalancesForPeriodInUsd(rootDepoAccount, period);

      foreach (DateTime day in period)
      {
        var cashInUsd = cashBalances[day];
        var depoInUsd = depoBalances[day];
        CashSeries.Add(new DateProcentPoint(day, Math.Round(cashInUsd / (cashInUsd + depoInUsd) * 100)));
        DepoSeries.Add(new DateProcentPoint(day, 100));
        
      }
    }

    #region // Visibility

    private Visibility _chart2Visibility;
    private Visibility _chart1Visibility;
    private Visibility _chart3Visibility;
    private Visibility _chart4Visibility;

    public Visibility Chart1Visibility
    {
      get { return _chart1Visibility; }
      set
      {
        if (Equals(value, _chart1Visibility)) return;
        _chart1Visibility = value;
        NotifyOfPropertyChange(() => Chart1Visibility);
      }
    }
    public Visibility Chart2Visibility
    {
      get { return _chart2Visibility; }
      set
      {
        if (Equals(value, _chart2Visibility)) return;
        _chart2Visibility = value;
        NotifyOfPropertyChange(() => Chart2Visibility);
      }
    }
    public Visibility Chart3Visibility
    {
      get { return _chart3Visibility; }
      set
      {
        if (Equals(value, _chart3Visibility)) return;
        _chart3Visibility = value;
        NotifyOfPropertyChange(() => Chart3Visibility);
      }
    }
    public Visibility Chart4Visibility
    {
      get { return _chart4Visibility; }
      set
      {
        if (Equals(value, _chart4Visibility)) return;
        _chart4Visibility = value;
        NotifyOfPropertyChange(() => Chart4Visibility);
      }
    }

    public void ExpandChart1() {ExpandChart(1);}
    public void ExpandChart2() {ExpandChart(2);}
    public void ExpandChart3() {ExpandChart(3);}
    public void ExpandChart4() {ExpandChart(4);}

    public void ExpandChart(int clickedChart)
    {
      if (clickedChart != 1) Chart1Visibility = TurnoverVisibility(Chart1Visibility);
      if (clickedChart != 2) Chart2Visibility = TurnoverVisibility(Chart2Visibility);
      if (clickedChart != 3) Chart3Visibility = TurnoverVisibility(Chart3Visibility);
      if (clickedChart != 4) Chart4Visibility = TurnoverVisibility(Chart4Visibility);
    }

    private Visibility TurnoverVisibility(Visibility visibility)
    {
      return visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
    }

    #endregion
  }
}
