﻿using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class BalanceVerificationViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private string _caption;
        private decimal _total;
        public List<VerificationLine> Lines { get; set; }
        public VerificationLine SelectedLine { get; set; }

        public BalanceVerificationViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _caption;
        }

        private TransactionModel[] _trans;
        private int _transIndex;
        public void Initialize(AccountItemModel accountItemModel)
        {
            Lines = new List<VerificationLine>();
            _total = 0;
            _trans = _dataModel.Transactions.Values
                .OrderBy(t => t.Timestamp)
                .Where(t => t.MyAccount.Id == accountItemModel.Id || 
                            (t.MySecondAccount != null && t.MySecondAccount.Id == accountItemModel.Id))
                .ToArray();
            _transIndex = 0;
            while (true)
            {
                var t = _trans[_transIndex];
                switch (t.Operation)
                {
                    case OperationType.Доход: RegisterIncome(t); break;
                    case OperationType.Расход: RegisterExpense(t); break;
                    case OperationType.Перенос: RegisterTransfer(t, accountItemModel); break;
                    case OperationType.Обмен: RegisterExcange(t, accountItemModel); break;
                }

                _transIndex++;
                if (_transIndex == _trans.Length)
                    break;
            }

            SelectedLine = Lines.FirstOrDefault();
            if (SelectedLine == null) return;

            _caption = $"{accountItemModel.Name}  {_total:#,0.##}";
        }

        private void RegisterIncome(TransactionModel tr)
        {
            Lines.Insert(0, new VerificationLine()
            {
                Amount = tr.Amount,
                Date = tr.Timestamp.ToString("dd/MMM"),
                Counterparty = tr.GetCounterpartyName(_dataModel),
                OperationType = OperationType.Доход,
                Text = tr.Comment,
            });
            _total = _total + tr.Amount;
        }

        private void RegisterExpense(TransactionModel tr)
        {
            var line = RegisterOneExpense(tr);
            _total = _total - tr.Amount;

            if (tr.Receipt == 0)
            {
                Lines.Insert(0, line);
            }
            else
            {
                var receiptId = tr.Receipt;
                line.Text = "чек:  " + line.Text;
                while (_transIndex < _trans.Length - 1 && 
                       _trans[_transIndex + 1].Timestamp.Date == tr.Timestamp.Date && 
                       _trans[_transIndex + 1].Receipt == receiptId)
                {
                    _transIndex++;
                    _total = _total - _trans[_transIndex].Amount;
                    line.Amount = line.Amount  - _trans[_transIndex].Amount;
                    line.Text = line.Text + " ; " + _trans[_transIndex].Comment;
                }
                Lines.Insert(0, line);
            }
        }

        private VerificationLine RegisterOneExpense(TransactionModel tr)
        {
            return new VerificationLine()
            {
                Amount = -tr.Amount,
                Date = tr.Timestamp.ToString("dd/MMM"),
                Counterparty = tr.GetCounterpartyName(_dataModel),
                OperationType = OperationType.Расход,
                Text = tr.Comment,
            };
        }

        private void RegisterTransfer(TransactionModel tr, AccountItemModel accountItemModel)
        {
            var amount = tr.MyAccount.Id == accountItemModel.Id ? -tr.Amount : tr.Amount;
            Lines.Insert(0, new VerificationLine()
            {
                Amount = amount,
                Date = tr.Timestamp.ToString("dd/MMM"),
                Counterparty = (tr.MyAccount.Id == accountItemModel.Id ? tr.MySecondAccount : tr.MyAccount).Name,
                OperationType = OperationType.Перенос,
                Text = tr.Comment,
            });
            _total = _total + amount;
        }

        private void RegisterExcange(TransactionModel tr, AccountItemModel accountItemModel)
        {
            var amount = tr.MyAccount.Id == accountItemModel.Id ? tr.Amount * -1 : tr.AmountInReturn;
            Lines.Insert(0, new VerificationLine()
            {
                Amount = amount,
                Date = tr.Timestamp.ToString("dd/MMM"),
                Counterparty = (tr.MyAccount.Id == accountItemModel.Id ? tr.MySecondAccount : tr.MyAccount).Name,
                OperationType = OperationType.Обмен,
                Text = tr.Comment,
            });
            _total = _total + amount;
        }

        public void CheckLine()
        {
            SelectedLine.IsChecked = !SelectedLine.IsChecked;
        }
    }
}
