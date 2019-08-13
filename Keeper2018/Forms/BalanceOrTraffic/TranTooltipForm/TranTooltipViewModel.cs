using System;
using System.Collections.Generic;
using Caliburn.Micro;

namespace Keeper2018
{
    public class TranTooltipViewModel : Screen
    {
        private readonly KeeperDb _db;
        private readonly TransactionModel _tranModel;

        public List<string> Names { get; set; } = new List<string>();
        public List<string> Lines { get; set; } = new List<string>();

        public TranTooltipViewModel(KeeperDb db, Transaction transaction)
        {
            _db = db;
            _tranModel = transaction.Map(_db.AcMoDict, -1);
            Initialize();
        }

        private void Initialize()
        {
            Names.Add("Timestamp: ");
            Lines.Add(_tranModel.Timestamp.ToString("dd-MM-yyyy HH:mm"));

            if (_tranModel.Operation == OperationType.Перенос || _tranModel.Operation == OperationType.Обмен)
            {
                Names.Add($"{_tranModel.Operation} с:");
                Names.Add(" на:");
                Lines.Add(_tranModel.MyAccount.Name);
                Lines.Add(_tranModel.MySecondAccount.Name);
            }

            Names.Add("Amount: ");
            var amount = _db.AmountInUsdWithRate(_tranModel.Timestamp, 
                _tranModel.Currency, _tranModel.Amount, out decimal rate);

            if (_tranModel.Currency != CurrencyCode.USD)
                amount += $" (rate {rate:0.####})";
            Lines.Add(amount);

            if (_tranModel.Operation == OperationType.Обмен)
            {
                Names.Add("Amount in return: ");
                var amountInReturn = _db.AmountInUsdWithRate(_tranModel.Timestamp, 
                    _tranModel.CurrencyInReturn, _tranModel.AmountInReturn, out decimal rateInReturn);

                if (_tranModel.CurrencyInReturn != CurrencyCode.USD)
                    amountInReturn += $" (rate {rateInReturn:0.####})";
                Lines.Add(amountInReturn);
            }

            var flag = true;
            foreach (var accountModel in _tranModel.Tags)
            {
                if (flag)
                {
                    Names.Add("Tags:");
                    flag = false;
                }
                else
                {
                    Names.Add("");
                }
                Lines.Add(accountModel.Name);
            }

            
            Names.Add("Comment: ");
            Lines.Add($"{_tranModel.Comment}");
        }

        public void Close()
        {
            TryClose();
        }
    }
}