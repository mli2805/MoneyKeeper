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
            Rows.Clear();
            Rows.AddRange(_dataModel.AcMoDict.Values
                .Where(a => !a.Is(393) && !a.Is(235) // закрытые
                                       && a.IsDeposit && a.Deposit.Card != null)
                .OrderBy(d=>d.Deposit.FinishDate)
                .Select(GetVm));

            Total = $"Всего {Rows.Count}";
            TotalMine = $"мои {Rows.Count(r => r.IsMine)} / не мои {Rows.Count(r => !r.IsMine)}";
            TotalVirtual = $"пластик {Rows.Count(r => !r.IsVirtual)} / виртуалки {Rows.Count(r => r.IsVirtual)}";
        }

        private PayCardVm GetVm(AccountModel account)
        {
            var depositOffer = _dataModel.DepositOffers.First(o => o.Id == account.Deposit.DepositOfferId);
            var calc = new TrafficOfAccountCalculator(_dataModel, account, new Period(new DateTime(2001,12,31), DateTime.Today.AddDays(1)));
            calc.EvaluateAccount();
            calc.DepositReportModel.Balance.Currencies.TryGetValue(depositOffer.MainCurrency, out var amount);
            return new PayCardVm()
            {
                CardNumber = account.Deposit.Card.CardNumber,
                CardHolder = account.Deposit.Card.CardHolder,
                IsMine = account.Deposit.Card.IsMine,
                PaymentSystem = account.Deposit.Card.PaymentSystem,
                IsVirtual = account.Deposit.Card.IsVirtual,
                IsPayPass = account.Deposit.Card.IsPayPass,

                AgreementNumber = account.Deposit.Serial,
                StartDate = account.Deposit.StartDate,
                FinishDate = account.Deposit.FinishDate,
                Comment = account.Deposit.Comment,

                BankAccount = _dataModel.AcMoDict.Values.First(a => a.Id == depositOffer.Bank.Id),
                MainCurrency = depositOffer.MainCurrency,

                Name = account.Name,
                Amount = amount,
            };
        }
    }
}
