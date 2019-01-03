using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class BalanceVerificationViewModel : Screen
    {
        private readonly KeeperDb _db;
        private string _caption;
        private decimal _total;
        public List<VerificationLine> Lines { get; set; }
        public VerificationLine SelectedLine { get; set; }

        public BalanceVerificationViewModel(KeeperDb db)
        {
            _db = db;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _caption;
        }

        public void Initialize(AccountModel accountModel)
        {
            Lines = new List<VerificationLine>();
            _total = 0;
            foreach (var tr in _db.Bin.Transactions.Values.
                Where(t => t.MyAccount == accountModel.Id || t.MySecondAccount == accountModel.Id))
            {
                switch (tr.Operation)
                {
                    case OperationType.Доход: RegisterIncome(tr); break;
                    case OperationType.Расход: RegisterExpense(tr); break;
                    case OperationType.Перенос: RegisterTransfer(tr, accountModel); break;
                    case OperationType.Обмен: RegisterExcange(tr, accountModel); break;
                }
            }

            if (_accumulatorReceiptId != 0)
            {
                Lines.Insert(0, _accumulator);
                _accumulatorReceiptId = 0;
            }

            SelectedLine = Lines.First();
            _caption = $"{accountModel.Name}  {_total:#,0.##}";
        }

        private void RegisterIncome(Transaction tr)
        {
            Lines.Insert(0, new VerificationLine()
            {
                Amount = tr.Amount,
                Date = tr.Timestamp.ToString("dd/MMM"),
                Counterparty = GetCounterparty(tr).Name,
                Text = tr.Comment,
            });
            _total = _total + tr.Amount;
        }

        private VerificationLine _accumulator;
        private int _accumulatorReceiptId;
        private void RegisterExpense(Transaction tr)
        {
            var line = RegisterOneExpense(tr);
            _total = _total - tr.Amount;
            if (tr.Receipt == 0)
            {
                Lines.Insert(0, line);
            }
            else
            {
                if (_accumulatorReceiptId == tr.Receipt)
                {
                    _accumulator.Amount = _accumulator.Amount + line.Amount;
                }
                else
                {
                    if (_accumulatorReceiptId != 0)
                        Lines.Insert(0, _accumulator);
                    _accumulator = line;
                    _accumulatorReceiptId = tr.Receipt;
                }
            }
        }

        private VerificationLine RegisterOneExpense(Transaction tr)
        {
            return new VerificationLine()
            {
                Amount = -tr.Amount,
                Date = tr.Timestamp.ToString("dd/MMM"),
                Counterparty = GetCounterparty(tr).Name,
                Text = tr.Comment,
            };
        }

        private void RegisterTransfer(Transaction tr, AccountModel accountModel)
        {
            var amount = tr.MyAccount == accountModel.Id ? -tr.Amount : tr.Amount;
            Lines.Insert(0, new VerificationLine()
            {
                Amount = amount,
                Date = tr.Timestamp.ToString("dd/MMM"),
                Counterparty = _db.AcMoDict[tr.MyAccount == accountModel.Id ? tr.MySecondAccount : tr.MyAccount].Name,
                Text = tr.Comment,
            });
            _total = _total + amount;
        }

        private void RegisterExcange(Transaction tr, AccountModel accountModel)
        {
            var amount = tr.MyAccount == accountModel.Id ? tr.Amount * -1 : tr.AmountInReturn;
            Lines.Insert(0, new VerificationLine()
            {
                Amount = amount,
                Date = tr.Timestamp.ToString("dd/MMM"),
                Counterparty = _db.AcMoDict[tr.MyAccount == accountModel.Id ? tr.MySecondAccount : tr.MyAccount].Name,
                Text = tr.Comment,
            });
            _total = _total + amount;
        }

        private AccountModel GetCounterparty(Transaction tr)
        {
            AccountModel counterparty = new AccountModel("контрагент не найден");
            foreach (var trTag in tr.Tags)
            {
                var tag = _db.AcMoDict[trTag];
                if (tag.Is(157)) // внешние
                    counterparty = tag;
            }
            return counterparty;
        }

        public void CheckLine()
        {
            SelectedLine.IsChecked = !SelectedLine.IsChecked;
        }
    }
}
