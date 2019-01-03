using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;

namespace Keeper2018
{
    public class VerificationLine : PropertyChangedBase
    {
        private bool _isChecked;

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (value == _isChecked) return;
                _isChecked = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(BackgroundBrush));
            }
        }

        public string Amount { get; set; }
        public string Date { get; set; }
        public string Counterparty { get; set; }
        public string Text { get; set; }
        public Brush BackgroundBrush => IsChecked ? Brushes.DarkGray : Brushes.White;
    }
    public class BalanceVerificationViewModel : Screen
    {
        private readonly KeeperDb _db;
        private string _caption;
        public List<VerificationLine> Lines { get; set; }
        public VerificationLine SelectedLine {get; set; }

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
            _caption = accountModel.Name;
            Lines = new List<VerificationLine>();
            foreach (var tr in _db.Bin.Transactions.Values.Where(t=>t.MyAccount == accountModel.Id || t.MySecondAccount == accountModel.Id))
            {
                if (tr.Receipt == 0)
                {
                    if (tr.MyAccount == accountModel.Id)
                    {
                        Lines.Insert(0, new VerificationLine()
                        {
                            Amount = tr.Amount.ToString("#,0.##"),
                            Date = tr.Timestamp.ToString("dd/MMM"),
                            Counterparty = GetCounterparty(tr).Name,
                            Text = tr.Comment,
                        });
                    }
                }
                 
            }

            SelectedLine = Lines.First();
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
