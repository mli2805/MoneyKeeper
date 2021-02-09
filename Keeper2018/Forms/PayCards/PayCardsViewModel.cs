using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018.PayCards
{
    public class PayCardsViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;

        public List<PayCardVm> Rows { get; set; } = new List<PayCardVm>();

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
                .Select(GetVm));
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
                PaymentSystem = account.Deposit.Card.PaymentSystem,
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
