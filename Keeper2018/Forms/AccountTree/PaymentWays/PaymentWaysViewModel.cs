using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class PaymentWaysViewModel : Screen
    {
        private readonly KeeperDb _db;
        private List<Transaction> _trans;
        public decimal Total { get; set; }

        public PaymentWaysViewModel(KeeperDb db)
        {
            _db = db;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Раскладка по способам оплаты";
        }

        public void Initialize(AccountModel accountModel)
        {
            var period = DateTime.Today.GetFullMonthForDate();
            _trans = _db.Bin.Transactions.Values.Where(t => t.Operation == OperationType.Расход
                                                        && t.MyAccount == accountModel.Id
                                                        && period.Includes(t.Timestamp)).ToList();
            Total = _trans.Sum(t => t.Amount);
        }

        public void Export()
        {
            var content = _trans.Select(t => t.Dump());
            File.WriteAllLines(@"c:\temp\joker.csv", content);
        }
        public void Close() { TryClose(); }

    }
}
