using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;
using Microsoft.Win32;

namespace Keeper2018
{
    public class PaymentWaysViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private AccountModel _cardAccountModel;
        private Period _period;
        private List<TransactionModel> _expenses;
        public ObservableCollection<string> Lines { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> Expenses { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> Incomes { get; set; } = new ObservableCollection<string>();
        public decimal Expense { get; set; }
        public decimal Income { get; set; }
        public decimal ETotal { get; set; }
        public decimal ITotal { get; set; }

        public PaymentWaysViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = $"{_cardAccountModel.Name}: Раскладка по способам оплаты   {ITotal} - {ETotal} = {ITotal - ETotal}";
        }

        public void Initialize(AccountModel accountModel)
        {
            _cardAccountModel = accountModel;
            _period = DateTime.Today.GetFullMonthForDate();
            Initialize();
        }

        private void Initialize()
        {
            var monthTrans = _dataModel.Transactions.Values
                .Where(t => _period.Includes(t.Timestamp)).ToList();

            _expenses = monthTrans.Where(t => t.Operation == OperationType.Расход
                                              && t.MyAccount.Id == _cardAccountModel.Id).ToList();

            foreach (var tran in _expenses.Where(t => t.PaymentWay == PaymentWay.НеЗадано))
                tran.PaymentWay = PaymentGuess.GuessPaymentWay(tran);

            Lines.Clear();
            Expenses.Clear();
            Incomes.Clear();
            StepOne(_expenses, Lines, Expenses);

            var transfersToCache = monthTrans
                .Where(t => (t.Operation == OperationType.Перенос || t.Operation == OperationType.Обмен)
                                                                  && t.MyAccount.Id == _cardAccountModel.Id
                                                                  && t.MySecondAccount.Is(160)).ToList();
            ETotal += StepTwo("Снято налом", transfersToCache, Lines, Expenses);

            var transfersToCards = monthTrans
                .Where(t => (t.Operation == OperationType.Перенос || t.Operation == OperationType.Обмен)
                                                                            && t.MyAccount.Id == _cardAccountModel.Id
                                                                            && t.MySecondAccount.Is(161)).ToList();
            ETotal += StepTwo("Переведено на карты", transfersToCards, Lines, Expenses);

            var incomes = monthTrans.Where(t => t.Operation == OperationType.Доход
                                                && t.MyAccount.Id == _cardAccountModel.Id).ToList();
            ITotal += StepTwo("Доходы", incomes, Lines, Incomes);

            var transfersFromCache = monthTrans
                .Where(t => (t.Operation == OperationType.Перенос || t.Operation == OperationType.Обмен)
                                                                        && t.MySecondAccount.Id == _cardAccountModel.Id
                                                                        && t.MyAccount.Is(160)).ToList();
            ITotal += StepTwo("Пополнено налом", transfersFromCache, Lines, Incomes);
           
            var transfersFromCards = monthTrans
                .Where(t => (t.Operation == OperationType.Перенос || t.Operation == OperationType.Обмен)
                                                                             && t.MySecondAccount.Id == _cardAccountModel.Id
                                                                             && t.MyAccount.Is(161)).ToList();
            ITotal += StepTwo("Пополнено с карт", transfersFromCards, Lines, Incomes);
        }

        private decimal StepTwo(string title, List<TransactionModel> source, ObservableCollection<string> lines, ObservableCollection<string> totals)
        {
            if (!source.Any()) return 0;

            var total = source.Sum(t => t.Amount);
            lines.Add($"       {title}:   {total}");
            totals.Add($"{title}:   {total}");
            foreach (var tr in source)
            {
                lines.Add(tr.Timestamp.ToString("dd/MM/yyyy HH:mm") + " ; " +
                          tr.Amount.ToString(new CultureInfo("en-US")) + " " + tr.Currency + " ; " +
                          StepTwoTags(tr) + " ; " + tr.Comment);
            }
            lines.Add("");
            return total;
        }

        private string StepTwoTags(TransactionModel tr)
        {
            if (tr.Operation != OperationType.Доход)
                return (_cardAccountModel.Id == tr.MyAccount.Id ? tr.MySecondAccount : tr.MyAccount).ToString();

            string result = "";
            foreach (var accountModel in tr.Tags)
            {
                result += accountModel + " | ";
            }
            return result;

        }

        private void StepOne(List<TransactionModel> source, ObservableCollection<string> lines, ObservableCollection<string> totals)
        {
            foreach (var paymentWay in Enum.GetValues(typeof(PaymentWay)).OfType<PaymentWay>().ToList())
            {
                var onePaymentWay = source.Where(t => t.PaymentWay == paymentWay).ToList();
                if (onePaymentWay.Any())
                {
                    var total = onePaymentWay.Sum(t => t.Amount);
                    lines.Add($"        {paymentWay}:   {total}");
                    totals.Add($"{paymentWay}:   {total}");
                    ETotal += total;
                    foreach (var receipt in ShrinkReceipts(onePaymentWay))
                        lines.Add(receipt);
                    lines.Add("");
                }
            }
        }

        public void StepMonthBefore()
        {
            _period = _period.StartDate.AddMonths(-1).GetFullMonthForDate();
            Initialize();
        }

        public void StepMonthAfter()
        {
            _period = _period.StartDate.AddMonths(1).GetFullMonthForDate();
            Initialize();
        }

        public void Export()
        {
            var keeperInDropboxFullPath = PathFinder.GetKeeper2018BasePath();
            var dbPath = keeperInDropboxFullPath + @"\Reports";

            var dlg = new SaveFileDialog { InitialDirectory = dbPath, DefaultExt = "csv", Filter = "Csv files|*.csv" };
            if (dlg.ShowDialog() != true)
                return;
            File.WriteAllLines(dlg.FileName, Lines);
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
