using Caliburn.Micro;
using KeeperDomain;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Keeper2018.CardFee
{
    public class CardFeeViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private readonly ShellPartsBinder _shellPartsBinder;

        public string BankLine { get; set; }
        public string CardLine { get; set; }
        public string CardCurrency { get; set; }

        public decimal Amount { get; set; }
        public DatePickerWithTrianglesVm MyDatePickerVm { get; set; }
        public string Comment { get; set; } = "";

        private AccountItemModel _card;
        private AccountItemModel _bank;

        public CardFeeViewModel(KeeperDataModel dataModel, ShellPartsBinder shellPartsBinder)
        {
            _dataModel = dataModel;
            _shellPartsBinder = shellPartsBinder;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Списана комиссия";
        }

        public void Initialize(AccountItemModel card)
        {
            _card = card;
            _bank = _dataModel.AcMoDict[card.BankAccount.BankId];
            BankLine = $"Банк \"{_bank.Name}\" списал комиссию";
            CardLine = $"с карты \"{card.Name}\"";
            CardCurrency = _card.BankAccount.MainCurrency.ToString().ToUpper();
            MyDatePickerVm = new DatePickerWithTrianglesVm() { SelectedDate = DateTime.Today };
        }

        public void Save()
        {
            var id = _dataModel.Transactions.Keys.Max() + 1;
            var thisDateTrans = _dataModel.Transactions.Values
                .Where(t => t.Timestamp.Date == MyDatePickerVm.SelectedDate)
                .OrderBy(l => l.Timestamp)
                .LastOrDefault();
            var timestamp = thisDateTrans?.Timestamp ?? MyDatePickerVm.SelectedDate;
            var tranModel1 = new TransactionModel()
            {
                Id = id,
                Timestamp = timestamp.AddMinutes(1),
                Operation = OperationType.Расход,
                MyAccount = _card,
                Amount = Amount,
                Currency = _card.BankAccount.MainCurrency,
                Tags = new List<AccountItemModel>() { _bank },
                Comment = Comment,
            };
            tranModel1.Tags.Add(_dataModel.CardFeeTag());
            _dataModel.Transactions.Add(tranModel1.Id, tranModel1);

            _shellPartsBinder.JustToForceBalanceRecalculation = DateTime.Now;

            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}
