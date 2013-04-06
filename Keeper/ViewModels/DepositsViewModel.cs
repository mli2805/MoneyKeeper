using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

    public ChartPoint() { }
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

    public DateProcentPoint() { }
    public DateProcentPoint(DateTime date, decimal procent)
    {
      Date = date;
      Procent = procent;
    }
  }

  public class DepositsViewModel : Screen
  {
    public static IWindowManager WindowManager { get { return IoC.Get<IWindowManager>(); } }
    public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    public List<Deposit> DepositsList { get; set; }
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
          var temp = new Deposit { Account = account };
          temp.CollectInfo();
          DepositsList.Add(temp);
        }
      }
      SelectedDeposit = DepositsList[0];
     
      UpperRow = new GridLength(1,GridUnitType.Star);
      LowerRow = new GridLength(1, GridUnitType.Star);
      LeftColumn = new GridLength(1, GridUnitType.Star);
      RightColumn = new GridLength(1, GridUnitType.Star);

      var sw = new Stopwatch();
      sw.Start();

      TotalBalancesCtor();
      YearsProfitCtor();
      DepoCurrenciesProportionChartCtor();
      CashDepoProportionChartCtor();

      sw.Stop();
      Console.WriteLine(sw.Elapsed);
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

    public List<ChartPoint> TotalsList { get; set; }
    public void TotalBalancesCtor()
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
              (int)totalBalances[currency]));
        else
        {
          var inUsd = totalBalances[currency] / (decimal)Rate.GetLastRate(currency);
          TotalsList.Add(
            new ChartPoint(
              String.Format("{0:#,0} {1}", totalBalances[currency], currency),
              (int)Math.Round(inUsd)));
        }
      }
    }

    public List<ChartPoint> YearsList { get; set; }
    public void YearsProfitCtor()
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
              String.Format("{0}\n {1:#,0}$/мес", i, yearTotal / 12),
              (int)yearTotal));
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

    //    public void CashDepoProportionChartCtor()
    //    {
    //      CashSeries = new List<DateProcentPoint>();
    //      DepoSeries = new List<DateProcentPoint>();
    //      var rootCashAccount = Db.FindAccountInTree("На руках");
    //      var rootDepoAccount = Db.FindAccountInTree("Депозиты");

    //      var period = new Period(new DateTime(2002, 1, 1), DateTime.Today);
    //      var cashBalances = Balance.AccountBalancesForPeriodInUsd(rootCashAccount, period);
    //      var depoBalances = Balance.AccountBalancesForPeriodInUsd(rootDepoAccount, period);

    //      foreach (DateTime day in period)
    //      {
    //        var cashInUsd = cashBalances[day];
    //        var depoInUsd = depoBalances[day];
    //        CashSeries.Add(new DateProcentPoint(day, Math.Round(cashInUsd / (cashInUsd + depoInUsd) * 100)));
    //        DepoSeries.Add(new DateProcentPoint(day, 100));

    //      }
    //    }

    public List<DateProcentPoint> MonthlyCashSeries { get; set; }

    //     на hall-comp 1-й вариант 102 сек, 2-й вариант 71 сек, 3-й вариант 1.4 сек
    //     на opx-lmarholin 1-й вариант 82 сек, 2-й вариант 57 сек, 3-й вариант 1.3 сек
    public void CashDepoProportionChartCtor()
    {
      var dailyCashSeries = new List<DateProcentPoint>();

      decimal cashInUsd = 0, depoInUsd = 0;
      var dt = new DateTime(2002, 1, 1);
      var transactionsArray = Db.Transactions.ToArray();
      int index = 0;
      Transaction tr = transactionsArray[0];

      while (index < transactionsArray.Count())
      {
        while (tr.Timestamp.Date == dt.Date)
        {
          if (tr.Debet.IsTheSameOrDescendantOf("На руках"))
          {
            cashInUsd -= tr.Currency == CurrencyCodes.USD ? tr.Amount : tr.Amount / (decimal)Rate.GetRate(tr.Currency, tr.Timestamp);
            if (tr.Operation == OperationType.Обмен)
              cashInUsd += tr.Currency2 == CurrencyCodes.USD
                             ? tr.Amount2
                             : tr.Amount2 / (decimal)Rate.GetRate((CurrencyCodes)tr.Currency2, tr.Timestamp);
          }
          if (tr.Credit.IsTheSameOrDescendantOf("На руках"))
            cashInUsd += tr.Currency == CurrencyCodes.USD ? tr.Amount : tr.Amount / (decimal)Rate.GetRate(tr.Currency, tr.Timestamp);
          if (tr.Debet.IsTheSameOrDescendantOf("Депозиты"))
            depoInUsd -= tr.Currency == CurrencyCodes.USD ? tr.Amount : tr.Amount / (decimal)Rate.GetRate(tr.Currency, tr.Timestamp);
          if (tr.Credit.IsTheSameOrDescendantOf("Депозиты"))
            depoInUsd += tr.Currency == CurrencyCodes.USD ? tr.Amount : tr.Amount / (decimal)Rate.GetRate(tr.Currency, tr.Timestamp);

          if (index == transactionsArray.Count() - 1) break;
          index++;
          tr = transactionsArray[index];
        }

        dailyCashSeries.Add(new DateProcentPoint(dt, Math.Round(cashInUsd / (cashInUsd + depoInUsd) * 100)));
        if (index == transactionsArray.Count() - 1) break;
        dt = dt.AddDays(1);
      }

      MonthlyCashSeries = (from p in dailyCashSeries
                           group p by new { year = p.Date.Year, month = p.Date.Month}
                             into g
                             select new DateProcentPoint
                             {
                               Date = new DateTime(g.Key.year,g.Key.month,15),
                               Procent = Math.Round(g.Average(a => a.Procent))
                             }).ToList();
    }

    #region // Fun with Charts Expand
    
    private GridLength _upperRow;
    private GridLength _lowerRow;
    private GridLength _leftColumn;
    private GridLength _rightColumn;
    public GridLength UpperRow
    {
      get { return _upperRow; }
      set
      {
        if (value.Equals(_upperRow)) return;
        _upperRow = value;
        NotifyOfPropertyChange(() => UpperRow);
      }
    }
    public GridLength LowerRow
    {
      get { return _lowerRow; }
      set
      {
        if (value.Equals(_lowerRow)) return;
        _lowerRow = value;
        NotifyOfPropertyChange(() => LowerRow);
      }
    }
    public GridLength LeftColumn
    {
      get { return _leftColumn; }
      set
      {
        if (value.Equals(_leftColumn)) return;
        _leftColumn = value;
        NotifyOfPropertyChange(() => LeftColumn);
      }
    }
    public GridLength RightColumn
    {
      get { return _rightColumn; }
      set
      {
        if (value.Equals(_rightColumn)) return;
        _rightColumn = value;
        NotifyOfPropertyChange(() => RightColumn);
      }
    }

    public void ExpandChart1()
    {
      LowerRow = TurnoverGridSize(LowerRow);
      RightColumn = TurnoverGridSize(RightColumn);
    }
    public void ExpandChart2()
    {
      LowerRow = TurnoverGridSize(LowerRow);
      LeftColumn = TurnoverGridSize(LeftColumn);
    }
    public void ExpandChart3()
    {
      UpperRow = TurnoverGridSize(UpperRow);
      RightColumn = TurnoverGridSize(RightColumn);
    }
    public void ExpandChart4()
    {
      UpperRow = TurnoverGridSize(UpperRow);
      LeftColumn = TurnoverGridSize(LeftColumn);
    }

    private GridLength TurnoverGridSize(GridLength size)
    {
      return size == new GridLength(0) ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
    }

    #endregion
  }
}
