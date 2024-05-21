using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class CardPaymentsLimitsViewModel : PropertyChangedBase
    {
        private readonly KeeperDataModel _keeperDataModel;
        public List<CardPaymentsLimitsModel> Rows { get; set; } = new List<CardPaymentsLimitsModel>();

        public CardPaymentsLimitsViewModel(KeeperDataModel keeperDataModel)
        {
            _keeperDataModel = keeperDataModel;
        }

        public void Initialize()
        {
            Rows.Clear();

            var ll = new List<CardPaymentsLimitsModel>();
            foreach (var a in _keeperDataModel.AcMoDict.Values
                         .Where(a => !a.Is(NickNames.Closed) && a.IsCard && a.BankAccount.MainCurrency == CurrencyCode.BYN))
            {
                var depoOffer = _keeperDataModel.DepositOffers.First(o=>o.Id == a.BankAccount.DepositOfferId);

                var line = new CardPaymentsLimitsModel()
                {
                    Account = a,
                    CurrentBalance = _keeperDataModel.GetCurrentBalance(a),
                    CurrentExpense = _keeperDataModel.GetExpenseForCurrentMonth(a),
                    ExpenseNotLess = depoOffer.MonthPaymentsMinimum,
                    ExpenseNotMore = depoOffer.MonthPaymentsMaximum,
                    Comment = depoOffer.Comment
                };

                ll.Add(line);
            }

            Rows.AddRange(ll.OrderByDescending(l=>l.CurrentBalance));
        }
    }
}
