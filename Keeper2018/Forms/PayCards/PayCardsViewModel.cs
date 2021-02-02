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
            Rows.AddRange(_dataModel.AccountsTree.SelectMany(GetActiveCards).Select(GetVm));
        }

        private List<AccountModel> GetActiveCards(AccountModel root)
        {
            var result = new List<AccountModel>();
            if (root.IsCard)
                result.Add(root);
            if (root.Id == 393 || root.Id == 235)
                return result;
            foreach (var child in root.Children)
                result.AddRange(GetActiveCards(child));
            return result;
        }

        private PayCardVm GetVm(AccountModel account)
        {
            var depositOffer = _dataModel.DepositOffers.First(o => o.Id == account.Deposit.DepositOfferId);
            var calc = new TrafficOfAccountCalculator(_dataModel, account, new Period(new DateTime(2001,12,31), DateTime.Today.AddDays(1)));
            calc.EvaluateAccount();
            calc.DepositReportModel.Balance.Currencies.TryGetValue(depositOffer.MainCurrency, out var amount);
            return new PayCardVm()
            {
                MyAccountId = account.Id,

                CardNumber = account.Deposit.Card.CardNumber,
                CardHolder = account.Deposit.Card.CardHolder,
                PaymentSystem = account.Deposit.Card.PaymentSystem,
                IsPayPass = account.Deposit.Card.IsPayPass,

                AgreementNumber = account.Deposit.Serial,
                StartDate = account.Deposit.StartDate,
                FinishDate = account.Deposit.FinishDate,
                Comment = account.Deposit.Comment,

                BankAccount = _dataModel.AccountPlaneList.First(a => a.Id == depositOffer.BankId),
                MainCurrency = depositOffer.MainCurrency,

                Name = account.Name,
                Amount = amount,
            };
        }
    }
}
