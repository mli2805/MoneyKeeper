using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using Keeper2018.BorderedList;
using KeeperDomain;
using Microsoft.Win32;

namespace Keeper2018
{
    public class PaymentWaysViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private AccountItemModel _cardAccountItemModel;
        private Period _period;
        // private List<TransactionModel> _otherExpenses;

        private BorderedListViewModel _transactions;
        public BorderedListViewModel Transactions
        {
            get => _transactions;
            set
            {
                if (Equals(value, _transactions)) return;
                _transactions = value;
                NotifyOfPropertyChange(() => Transactions);
            }
        }

        private BorderedListViewModel _totalFrom;
        public BorderedListViewModel TotalFrom
        {
            get => _totalFrom;
            set
            {
                if (Equals(value, _totalFrom)) return;
                _totalFrom = value;
                NotifyOfPropertyChange(() => TotalFrom);
            }
        }

        private BorderedListViewModel _totalTo;
        public BorderedListViewModel TotalTo
        {
            get => _totalTo;
            set
            {
                if (Equals(value, _totalTo)) return;
                _totalTo = value;
                NotifyOfPropertyChange(() => TotalTo);
            }
        }

        private decimal _sumFrom;
        private decimal _sumTo;

        private string _currentPeriodName;
        public string CurrentPeriodName
        {
            get => _currentPeriodName;
            set
            {
                if (value == _currentPeriodName) return;
                _currentPeriodName = value;
                NotifyOfPropertyChange(() => CurrentPeriodName);
            }
        }

        public string Total => $"Итого: {_sumTo} - {_sumFrom} = {_sumTo - _sumFrom}";

        public PaymentWaysViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = $"{_cardAccountItemModel.Name}: Раскладка по способам оплаты";
        }

        public void Initialize(AccountItemModel accountItemModel)
        {
            _cardAccountItemModel = accountItemModel;
            _period = DateTime.Today.GetFullMonthForDate();
            _currentPeriodName = $"{_period.StartDate:MMM yyyy}";
            Initialize();
        }

        private void Initialize()
        {
            _sumFrom = 0;
            _sumTo = 0;
            var transactions = new BorderedListViewModel(150);
            var totalFrom = new BorderedListViewModel(75);
            var totalTo = new BorderedListViewModel(75);

            var monthTrans = _dataModel.Transactions.Values
                .Where(t => _period.Includes(t.Timestamp)).ToList();

            var cardFees = monthTrans.Where(t => t.Operation == OperationType.Расход
                                              && t.MyAccount.Id == _cardAccountItemModel.Id
                                              && t.Tags.Contains(_dataModel.CardFeeTag())
                                              ).ToList();
            _sumTo += StepTwo("Комса с карты", cardFees, transactions.List, totalFrom.List, Brushes.Red);

            var otherExpenses = monthTrans.Where(t => t.Operation == OperationType.Расход
                                                      && t.MyAccount.Id == _cardAccountItemModel.Id
                                                      && !t.Tags.Contains(_dataModel.CardFeeTag())
                                                        ).ToList();
            foreach (var tran in otherExpenses.Where(t => t.PaymentWay == PaymentWay.НеЗадано))
                tran.PaymentWay = PaymentGuess.GuessPaymentWay(tran);

            StepOne(otherExpenses, transactions.List, totalFrom.List);

            var transfersToCache = monthTrans
                .Where(t => (t.Operation == OperationType.Перенос || t.Operation == OperationType.Обмен)
                                                                  && t.MyAccount.Id == _cardAccountItemModel.Id
                                                                  && t.MySecondAccount.Is(160)).ToList();
            _sumFrom += StepTwo("Снято налом", transfersToCache, transactions.List, totalFrom.List, Brushes.Black);

            var transfersToCards = monthTrans
                .Where(t => (t.Operation == OperationType.Перенос || t.Operation == OperationType.Обмен)
                                                                            && t.MyAccount.Id == _cardAccountItemModel.Id
                                                                            && t.MySecondAccount.Is(161)).ToList();
            _sumFrom += StepTwo("Переведено на карты", transfersToCards, transactions.List, totalFrom.List, Brushes.Black);


            var moneyBacks = monthTrans.Where(t => t.Operation == OperationType.Доход
                                                            && t.MyAccount.Id == _cardAccountItemModel.Id
                                                            && t.Tags.Contains(_dataModel.MoneyBackTag())).ToList();
            var percents = monthTrans.Where(t => t.Operation == OperationType.Доход
                                                                       && t.MyAccount.Id == _cardAccountItemModel.Id
                                                                       && t.Tags.Contains(_dataModel.PercentsTag())).ToList();
            var otherIncomes = monthTrans.Where(t => t.Operation == OperationType.Доход
                                                     && t.MyAccount.Id == _cardAccountItemModel.Id
                                                     && !t.Tags.Contains(_dataModel.MoneyBackTag())
                                                     && !t.Tags.Contains(_dataModel.PercentsTag())
                                                     ).ToList();

            _sumTo += StepTwo("Манибэк", moneyBacks, transactions.List, totalTo.List, Brushes.Blue);
            _sumTo += StepTwo("Проценты", percents, transactions.List, totalTo.List, Brushes.Blue);
            _sumTo += StepTwo("Другие доходы", otherIncomes, transactions.List, totalTo.List, Brushes.Blue);

            var transfersFromCache = monthTrans
                .Where(t => (t.Operation == OperationType.Перенос || t.Operation == OperationType.Обмен)
                                                                        && t.MySecondAccount.Id == _cardAccountItemModel.Id
                                                                        && t.MyAccount.Is(160)).ToList();
            _sumTo += StepTwo("Пополнено налом", transfersFromCache, transactions.List, totalTo.List, Brushes.Black);

            var transfersFromCards = monthTrans
                .Where(t => (t.Operation == OperationType.Перенос || t.Operation == OperationType.Обмен)
                                                                             && t.MySecondAccount.Id == _cardAccountItemModel.Id
                                                                             && t.MyAccount.Is(161)).ToList();
            _sumTo += StepTwo("Пополнено с карт", transfersFromCards, transactions.List, totalTo.List, Brushes.Black);

            Transactions = transactions;
            TotalFrom = totalFrom;
            TotalTo = totalTo;
        }


        private decimal StepTwo(string title, List<TransactionModel> source, ListOfLines lines, ListOfLines totals, Brush brush)
        {
            if (!source.Any()) return 0;

            var total = source.Sum(t => t.Amount);
            lines.Add($"       {title}:   {total}", brush);
            totals.Add($"{title}:   {total}", brush);
            foreach (var tr in source)
            {
                lines.Add(tr.Timestamp.ToString("dd/MM/yyyy HH:mm") + " ; " +
                          tr.Amount.ToString(new CultureInfo("en-US")) + " " + tr.Currency + " ; " +
                          StepTwoTags(tr) + " ; " + tr.Comment, brush);
            }
            lines.Add("");
            return total;
        }

        private string StepTwoTags(TransactionModel tr)
        {
            if (tr.Operation != OperationType.Доход && tr.Operation != OperationType.Расход)
                return (_cardAccountItemModel.Id == tr.MyAccount.Id ? tr.MySecondAccount : tr.MyAccount).ToString();

            string result = "";
            foreach (var accountModel in tr.Tags)
            {
                result += accountModel + " | ";
            }
            return result;

        }

        private void StepOne(List<TransactionModel> source, ListOfLines lines, ListOfLines totals)
        {
            foreach (var paymentWay in Enum.GetValues(typeof(PaymentWay)).OfType<PaymentWay>().ToList())
            {
                var onePaymentWay = source.Where(t => t.PaymentWay == paymentWay).ToList();
                if (onePaymentWay.Any())
                {
                    var total = onePaymentWay.Sum(t => t.Amount);
                    lines.Add($"        {paymentWay}:   {total}", Brushes.Red);
                    totals.Add($"{paymentWay}:   {total}", Brushes.Red);
                    _sumFrom += total;
                    foreach (var receipt in ShrinkReceipts(onePaymentWay))
                        lines.Add(receipt, Brushes.Red);
                    lines.Add("");
                }
            }
        }

        public void StepMonthBefore()
        {
            _period = _period.StartDate.AddMonths(-1).GetFullMonthForDate();
            CurrentPeriodName = $"{_period.StartDate:MMM yyyy}";
            Initialize();
            NotifyOfPropertyChange(nameof(Total));
        }

        public void StepMonthAfter()
        {
            _period = _period.StartDate.AddMonths(1).GetFullMonthForDate();
            CurrentPeriodName = $"{_period.StartDate:MMM yyyy}";
            Initialize();
            NotifyOfPropertyChange(nameof(Total));
        }

        public void Export()
        {
            var keeperInDropboxFullPath = PathFinder.GetKeeper2018BasePath();
            var dbPath = keeperInDropboxFullPath + @"\Reports";

            var dlg = new SaveFileDialog { InitialDirectory = dbPath, DefaultExt = "csv", Filter = "Csv files|*.csv" };
            if (dlg.ShowDialog() != true)
                return;
            File.WriteAllLines(dlg.FileName, Transactions.List.Lines.Select(l => l.Line));
        }
        public void Close() { TryClose(); }

        private List<string> ShrinkReceipts(List<TransactionModel> trans)
        {
            var singles = trans.Where(t => t.Receipt == 0).Select(t => new ExpenseTran(t)).ToList();
            var groups = trans.Where(t => t.Receipt > 0).GroupBy(t => t.Receipt.ToString() + "_" + t.Timestamp.Date).ToList();
            foreach (var group in groups)
            {
                if (group == null) continue; // just because of warning
                var tr = new ExpenseTran(group.AsEnumerable().First())
                {
                    Amount = group.Sum(t => t.Amount),
                    Comment = $"чек {group.Key.Substring(0, group.Key.IndexOf('_'))}"
                };
                singles.Add(tr);
            }
            return singles.OrderBy(t => t.Timestamp).Select(t => t.ToString(_dataModel)).ToList();
        }
    }
}
