using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Deposit;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Extentions;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.DepositProcessing;
using Keeper.Utils.DiagramDataExtraction;
using Keeper.Utils.Rates;

namespace Keeper.ViewModels.Deposits
{
    [Export]
    public class DepositsViewModel : Screen
    {
        public static IWindowManager WindowManager { get { return IoC.Get<IWindowManager>(); } }

        private readonly KeeperDb _db;
        private readonly RateExtractor _rateExtractor;
        private readonly DepositCalculationAggregator _depositCalculatorAggregator;
        private readonly OldRatesDiagramDataExtractor _oldRatesDiagramDataExtractor;

        public List<Deposit> DepositList { get; set; }
        public Deposit SelectedDeposit { get; set; }
        public List<DepositViewModel> LaunchedViewModels { get; set; }

        public Style MyTitleStyle { get; set; }

        [ImportingConstructor]
        public DepositsViewModel(KeeperDb db, RateExtractor rateExtractor,
                                 DepositCalculationAggregator depositCalculatorAggregator, 
                                 OldRatesDiagramDataExtractor oldRatesDiagramDataExtractor)
        {
            var sw = new Stopwatch();
            Console.WriteLine("DepositViewModel ctor starts {0}", sw.Elapsed);
            sw.Start();

            _db = db;
            _rateExtractor = rateExtractor;
            _depositCalculatorAggregator = depositCalculatorAggregator;
            _oldRatesDiagramDataExtractor = oldRatesDiagramDataExtractor;

            MyTitleStyle = new Style();

            DepositList = new List<Deposit>();
            foreach (var account in _db.FlattenAccounts())
            {
                if (!account.Is("Депозиты") || account.Children.Count != 0 || !account.IsDeposit()) continue;
                _depositCalculatorAggregator.FillinFieldsForOneDepositReport(account.Deposit);
                DepositList.Add(account.Deposit);
            }
            SelectedDeposit = DepositList[0];

            UpperRow = new GridLength(1, GridUnitType.Star);
            LowerRow = new GridLength(1, GridUnitType.Star);
            LeftColumn = new GridLength(1, GridUnitType.Star);
            RightColumn = new GridLength(1, GridUnitType.Star);

                  DepoCurrenciesProportionChartCtor();
            Console.WriteLine("DepositViewModel ctor 1 {0}", sw.Elapsed);
            YearsProfitCtor();
            Console.WriteLine("DepositViewModel ctor 2 {0}", sw.Elapsed);
            TotalBalancesCtor();
            Console.WriteLine("DepositViewModel ctor 3 {0}", sw.Elapsed);
//            CashDepoProportionChartCtor();

            sw.Stop();
            Console.WriteLine("DepositViewModel ctor takes {0}", sw.Elapsed);
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
                                   where d.Deposit.ParentAccount == SelectedDeposit.ParentAccount
                                   select d).FirstOrDefault();
                if (depositView != null)
                    depositView.TryClose();
            }
            var depositViewModel = IoC.Get<DepositViewModel>();
            depositViewModel.SetAccount(SelectedDeposit.ParentAccount);
            LaunchedViewModels.Add(depositViewModel);
            WindowManager.ShowWindow(depositViewModel);
        }


        #region // подготовка данных для чартов

        // распределение вкладов по валютам в течении периода наблюдений
        public List<DateProcentPoint> SeriesUsd { get; set; }
        public List<DateProcentPoint> SeriesBelo { get; set; }
        public List<DateProcentPoint> SeriesEuro { get; set; }
        public void DepoCurrenciesProportionChartCtor()
        {

            SeriesUsd = new List<DateProcentPoint>();
            SeriesBelo = new List<DateProcentPoint>(); // Byr, Byn
            SeriesEuro = new List<DateProcentPoint>();

            var inMoney = _oldRatesDiagramDataExtractor.DepositBalancesForPeriodInCurrencies(new Period(new DateTime(2001, 12, 31), DateTime.Now));
            foreach (var pair in inMoney)
            {
                var date = pair.Key;
                var balancesInCurrencies = pair.Value;

                var dateTotalInUsd = _rateExtractor.GetUsdEquivalent(balancesInCurrencies, date);
                decimal cumulativePercent;
                {
                    var inUsd = _rateExtractor.GetUsdEquivalent(balancesInCurrencies[CurrencyCodes.EUR], CurrencyCodes.EUR, date);
                    cumulativePercent = Math.Round(inUsd / dateTotalInUsd * 10000) / 100;
                    SeriesEuro.Add(new DateProcentPoint(date, cumulativePercent));
                }

                {
                    var inUsd = date < new DateTime(2016, 7, 1)
                        ? _rateExtractor.GetUsdEquivalent(balancesInCurrencies[CurrencyCodes.BYR], CurrencyCodes.BYR, date)
                        : _rateExtractor.GetUsdEquivalent(balancesInCurrencies[CurrencyCodes.BYN], CurrencyCodes.BYN, date);
                    cumulativePercent += Math.Round(inUsd / dateTotalInUsd * 10000) / 100;
                    SeriesBelo.Add(new DateProcentPoint(date, cumulativePercent));
                }

                SeriesUsd.Add(new DateProcentPoint(date, 100));
            }
        }

        // суммы прибыли от депозитов по годам
        public List<ChartPoint> YearsList { get; set; }
        public void YearsProfitCtor()
        {
            YearsList = new List<ChartPoint>();

            for (int i = 2002; i <= DateTime.Today.Year; i++)
            {
                decimal yearTotal = 0;
                foreach (var deposit in DepositList)
                {
                    yearTotal += _depositCalculatorAggregator.GetProfitForYear(deposit, i);
                }
                if (yearTotal != 0)
                    YearsList.Add(
                      new ChartPoint(
                          $"{i}\n {yearTotal/12:#,0}$/мес",
                        (int)yearTotal));
            }
        }

        // распределение вкладов по валютам в данный момент
        public List<ChartPoint> TotalsList { get; set; }
        public void TotalBalancesCtor()
        {
            TotalsList = new List<ChartPoint>();
            var totalBalances = new Dictionary<CurrencyCodes, decimal>();

            foreach (var deposit in DepositList)
            {
                if (deposit.CalculationData.CurrentBalance == 0) continue;
                decimal total;
                if (totalBalances.TryGetValue(deposit.DepositOffer.Currency, out total))
                    totalBalances[deposit.DepositOffer.Currency] = total + deposit.CalculationData.CurrentBalance;
                else
                    totalBalances.Add(deposit.DepositOffer.Currency, deposit.CalculationData.CurrentBalance);
            }

            foreach (var currency in totalBalances.Keys)
            {
                if (currency == CurrencyCodes.USD)
                    TotalsList.Add(
                      new ChartPoint(
                          $"{totalBalances[currency]:#,0} {currency}",
                        (int)totalBalances[currency]));
                else
                {
                    var inUsd = totalBalances[currency] / (decimal)_rateExtractor.GetLastRate(currency);
                    TotalsList.Add(
                      new ChartPoint(
                          $"{totalBalances[currency]:#,0} {currency}",
                        (int)Math.Round(inUsd)));
                }
            }
        }

        // соотношение депозитов и денег наруках
        public List<DateProcentPoint> MonthlyCashSeries { get; set; }
        public void CashDepoProportionChartCtor()
        {
//            var dailyCashSeries = new List<DateProcentPoint>();
//
//            decimal cashInUsd = 0, depoInUsd = 0;
//            var dt = new DateTime(2001, 12, 31);
//            var transactionsArray = Db.Transactions.OrderBy(t => t.Timestamp).ToArray();
//            int index = 0;
//            Transaction tr = transactionsArray[0];
//
//            while (index < transactionsArray.Count())
//            {
//                while (tr.Timestamp.Date == dt.Date)
//                {
//                    if (tr.Credit.Is("На руках"))
//                        cashInUsd += tr.Currency == CurrencyCodes.USD ? tr.Amount : tr.Amount / (decimal)_rateExtractor.GetRate(tr.Currency, tr.Timestamp);
//                    if (tr.Debet.Is("Депозиты"))
//                        depoInUsd -= tr.Currency == CurrencyCodes.USD ? tr.Amount : tr.Amount / (decimal)_rateExtractor.GetRate(tr.Currency, tr.Timestamp);
//                    if (tr.Credit.Is("Депозиты"))
//                        depoInUsd += tr.Currency == CurrencyCodes.USD ? tr.Amount : tr.Amount / (decimal)_rateExtractor.GetRate(tr.Currency, tr.Timestamp);
//
//                    index++;
//                    if (index == transactionsArray.Count()) break;
//                    tr = transactionsArray[index];
//                }
//
//                dailyCashSeries.Add(new DateProcentPoint(dt, Math.Round(cashInUsd / (cashInUsd + depoInUsd) * 100)));
//                if (index >= transactionsArray.Count()) break;
//                dt = dt.AddDays(1);
//            }
//
//            // средняя по месяцам
//            MonthlyCashSeries = (from p in dailyCashSeries
//                                 group p by new { year = p.Date.Year, month = p.Date.Month }
//                                     into g
//                                     select new DateProcentPoint
//                                     {
//                                         Date = new DateTime(g.Key.year, g.Key.month, 15),
//                                         Procent = Math.Round(g.Average(a => a.Procent))
//                                     }).ToList();
        }

        #endregion

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
