using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class PayCardsViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;

        public ObservableCollection<PayCardVm> Rows { get; set; } = new ObservableCollection<PayCardVm>();
        public ObservableCollection<string> Totals { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> Balance { get; set; } = new ObservableCollection<string>();

        public PayCardFilterVm Filter { get; set; }

        public PayCardsViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Платежные карты";
        }

        private void Filter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BalanceStr") return;

            RefreshTables();
        }

        public void Initialize()
        {
            Filter  = new PayCardFilterVm();
            Filter.PropertyChanged += Filter_PropertyChanged;
            RefreshTables();
        }

        private void RefreshTables()
        {
            Rows.Clear();
            Totals.Clear();
            Balance.Clear();

            _dataModel.GetActiveCardsOrderedByFinishDate().Select(GetVm).Where(Filter.Allow).ToList().ForEach(c => Rows.Add(c));

            Totals.Add($"Всего {Rows.Count}");
            Totals.Add($"мои {Rows.Count(r => r.IsMine)} / не мои {Rows.Count(r => !r.IsMine)}");
            Totals.Add($"пластик {Rows.Count(r => !r.IsVirtual)} / виртуалки {Rows.Count(r => r.IsVirtual)}");

            if (Filter.SelectedCurrency != "все")
                Balance.Add($"{Rows.Sum(r=>r.Amount)} {Rows.First().MainCurrency.ToString().ToLower()}");
        }

        private PayCardVm GetVm(AccountItemModel account)
        {
            try
            {
                var calc = new TrafficOfAccountCalculator(_dataModel, account, new Period(new DateTime(2001, 12, 31), DateTime.Today.AddDays(1)));
                calc.EvaluateAccount();
                calc.TryGetValue(account.BankAccount.MainCurrency, out var amount);
                return new PayCardVm()
                {
                    CardNumber = account.BankAccount.PayCard.CardNumber,
                    CardHolder = account.BankAccount.PayCard.CardHolder,
                    IsMine = account.BankAccount.IsMine,
                    PaymentSystem = account.BankAccount.PayCard.PaymentSystem,
                    IsVirtual = account.BankAccount.PayCard.IsVirtual,
                    IsPayPass = account.BankAccount.PayCard.IsPayPass,

                    AgreementNumber = account.BankAccount.AgreementNumber,
                    StartDate = account.BankAccount.StartDate,
                    FinishDate = account.BankAccount.FinishDate,
                    // Comment = account.BankAccount.PayCard.Comment,

                    BankAccount = _dataModel.AcMoDict.Values.First(a => a.Id == account.BankAccount.BankId),
                    MainCurrency = account.BankAccount.MainCurrency,

                    Name = account.Name,
                    Amount = amount,
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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
