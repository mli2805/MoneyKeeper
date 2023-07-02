using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018.PayCards
{
    public class PayCardsViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;

        public List<PayCardVm> Rows { get; set; } = new List<PayCardVm>();
        public string Total { get; set; }
        public string TotalMine { get; set; }
        public string TotalVirtual { get; set; }

        public PayCardsViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Initialize()
        {
            Rows= _dataModel.GetActiveCardsOrderedByFinishDate().Select(GetVm).ToList();

            Total = $"Всего {Rows.Count}";
            TotalMine = $"мои {Rows.Count(r => r.IsMine)} / не мои {Rows.Count(r => !r.IsMine)}";
            TotalVirtual = $"пластик {Rows.Count(r => !r.IsVirtual)} / виртуалки {Rows.Count(r => r.IsVirtual)}";
        }

        private PayCardVm GetVm(AccountItemModel account)
        {
            var depositOffer = _dataModel.DepositOffers.First(o => o.Id == account.Deposit.DepositOfferId);
            var calc = new TrafficOfAccountCalculator(_dataModel, account, new Period(new DateTime(2001,12,31), DateTime.Today.AddDays(1)));
            calc.EvaluateAccount();
            calc.DepositReportModel.Balance.Currencies.TryGetValue(depositOffer.MainCurrency, out var amount);
            return new PayCardVm()
            {
                CardNumber = account.PayCard.CardNumber,
                CardHolder = account.PayCard.CardHolder,
                IsMine = account.PayCard.IsMine,
                PaymentSystem = account.PayCard.PaymentSystem,
                IsVirtual = account.PayCard.IsVirtual,
                IsPayPass = account.PayCard.IsPayPass,

                AgreementNumber = account.PayCard.Serial,
                StartDate = account.PayCard.StartDate,
                FinishDate = account.PayCard.FinishDate,
                Comment = account.PayCard.Comment,

                BankAccount = _dataModel.AcMoDict.Values.First(a => a.Id == depositOffer.Bank.Id),
                MainCurrency = depositOffer.MainCurrency,

                Name = account.Name,
                Amount = amount,
            };
        }
    }
}
