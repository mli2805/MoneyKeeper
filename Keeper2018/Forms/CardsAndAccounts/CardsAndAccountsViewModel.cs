using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;
using Serilog;

namespace Keeper2018
{
    public class CardsAndAccountsViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;

        public ObservableCollection<CardOrAccountVm> Rows { get; set; } = new ObservableCollection<CardOrAccountVm>();
        public ObservableCollection<string> Totals { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> Balance { get; set; } = new ObservableCollection<string>();

        public CardsAndAccountsFilter Filter { get; set; }

        public CardsAndAccountsViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Платежные карты и счета";
        }

        private void Filter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BalanceStr") return;

            RefreshTables();
        }

        public void Initialize()
        {
            Filter  = new CardsAndAccountsFilter();
            Filter.PropertyChanged += Filter_PropertyChanged;
            RefreshTables();
        }

        private void RefreshTables()
        {
            Rows.Clear();
            Totals.Clear();
            Balance.Clear();

            _dataModel.GetCardsAndAccountsOrderedByFinishDate().Select(GetVm).Where(Filter.Allow).ToList().ForEach(c => Rows.Add(c));

            Totals.Add($"Всего {Rows.Count}");
            Totals.Add($"мои {Rows.Count(r => r.IsMine)} / не мои {Rows.Count(r => !r.IsMine)}");
            Totals.Add($"пластик {Rows.Count(r => !r.IsVirtual)} / виртуалки {Rows.Count(r => r.IsVirtual)}");

            if (Filter.SelectedCurrency != "все")
                Balance.Add($"{Rows.Sum(r=>r.Amount)} {Rows.First().MainCurrency.ToString().ToLower()}");
        }

        private CardOrAccountVm GetVm(AccountItemModel account)
        {
            try
            {
                var calc = new TrafficOfAccountCalculator(_dataModel, account, 
                    new Period(new DateTime(2001, 12, 31), DateTime.Today.AddDays(1)));
                calc.EvaluateAccount();
                calc.TryGetValue(account.BankAccount.MainCurrency, out var amount);
                var lineVm = new CardOrAccountVm()
                {
                    IsMine = account.BankAccount.IsMine,

                    AgreementNumber = account.BankAccount.AgreementNumber,
                    StartDate = account.BankAccount.StartDate,
                    FinishDate = account.BankAccount.FinishDate,

                    AccountItemOfBank = _dataModel.AcMoDict.Values.First(a => a.Id == account.BankAccount.BankId),
                    MainCurrency = account.BankAccount.MainCurrency,

                    Name = account.Name,
                    Amount = amount,
                };

                if (account.IsCard)
                {
                    lineVm.CardNumber = account.BankAccount.PayCard.CardNumber;
                    lineVm.CardHolder = account.BankAccount.PayCard.CardHolder;
                    lineVm.PaymentSystem = account.BankAccount.PayCard.PaymentSystem;
                    lineVm.IsVirtual = account.BankAccount.PayCard.IsVirtual;
                    lineVm.IsPayPass = account.BankAccount.PayCard.IsPayPass;
                }
                return lineVm;
            }
            catch (Exception e)
            {
                Log.Error(e, "CardsAndAccountsViewModel::GetVm");
                throw;
            }
        }

        public void ShowAll()
        {
            Filter.ClearAll();
        }

        public void ShowAllByn5()
        {
            Filter.ShowAllByn5();
        }

        public void ShowMineByn5()
        {
            Filter.ShowMineByn5();
        }
    }
}
