using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private List<TransactionModel> _trans;
        public ObservableCollection<string> Lines { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> Totals { get; set; } = new ObservableCollection<string>();
        public decimal Total { get; set; }

        public PaymentWaysViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Раскладка по способам оплаты";
        }

        public void Initialize(AccountModel accountModel)
        {
            _cardAccountModel = accountModel;
            _period = DateTime.Today.GetFullMonthForDate();
            Initialize();
        }

        private void Initialize()
        {
            _trans = _dataModel.Transactions.Values.Where(t => t.Operation == OperationType.Расход
                                                            && t.MyAccount.Id == _cardAccountModel.Id
                                                            && _period.Includes(t.Timestamp)).ToList();

            foreach (var tran in _trans.Where(t => t.PaymentWay == PaymentWay.НеЗадано))
                tran.PaymentWay = PaymentGuess.GuessPaymentWay(tran);

            Lines.Clear();
            Totals.Clear();
            foreach (var paymentWay in Enum.GetValues(typeof(PaymentWay)).OfType<PaymentWay>().ToList())
            {
                var onePaymentWay = _trans.Where(t => t.PaymentWay == paymentWay).ToList();
                if (onePaymentWay.Any())
                {
                    Lines.Add($"        {paymentWay}:   {onePaymentWay.Sum(t => t.Amount)}");
                    Totals.Add($"{paymentWay}:   {onePaymentWay.Sum(t => t.Amount)}");
                    foreach (var receipt in ShrinkReceipts(onePaymentWay))
                        Lines.Add(receipt);
                    Lines.Add("");
                }
            }

            Totals.Add($"Всего по карте:   {_trans.Sum(t => t.Amount)}");
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
