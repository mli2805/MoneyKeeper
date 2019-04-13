using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018.PayCards
{
    public class PayCardsViewModel : Screen
    {
        private readonly KeeperDb _db;

        public List<PayCardVm> Rows { get; set; } = new List<PayCardVm>();

        public PayCardsViewModel(KeeperDb db)
        {
            _db = db;
        }

        public void Initialize()
        {
            Rows.Clear();
            Rows.AddRange(_db.Bin.AccountPlaneList.Where(a => a.IsCard).Select(GetVm));
        }

        private PayCardVm GetVm(Account account)
        {
            var depositOffer = _db.Bin.DepositOffers.First(o => o.Id == account.Deposit.DepositOfferId);

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

                BankAccount = _db.Bin.AccountPlaneList.First(a => a.Id == depositOffer.Bank),
                MainCurrency = depositOffer.MainCurrency,

                Name = account.Name,
            };
        }
    }
}
