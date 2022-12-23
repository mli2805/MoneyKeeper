using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class OpenDepositsViewModel : Screen
    {
        private readonly KeeperDataModel _keeperDataModel;
        public List<DepositVm> Rows { get; private set; }

        public List<string> Footer { get; private set; }
        public List<string> Footer2 { get; private set; }

        public OpenDepositsViewModel(KeeperDataModel keeperDataModel)
        {
            _keeperDataModel = keeperDataModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Действующие депозиты";
        }

        public void Initialize()
        {
            Rows = _keeperDataModel.AcMoDict.Values
                .Where(a => !a.Children.Any() && a.Is(166) && !a.Is(235))
                .OrderBy(d => d.Deposit.FinishDate)
                .Select(Convert).ToList();

            Footer = new List<string>();
            int totalCount = 0;
            decimal total = 0;
            foreach (var currency in Enum.GetValues(typeof(CurrencyCode)).OfType<CurrencyCode>())
            {
                var dcs = Rows.Where(r => r.MainCurrency == currency).ToList();
                var sum = dcs.Sum(d => d.Balance.Currencies[currency]);
                if (sum > 0)
                {
                    totalCount += dcs.Count;
                    total += _keeperDataModel.AmountInUsd(DateTime.Now, currency, sum);
                    Footer.Add($"{dcs.Count} депо;  {sum:N} {currency.ToString().ToLower()}");
                }
            }

            Footer2 = new List<string> { $"Всего {totalCount} депо;   {total:N} usd" };
        }

        private DepositVm Convert(AccountModel accountModel)
        {
            var depoOffer = _keeperDataModel.DepositOffers
                .First(o => o.Id == accountModel.Deposit.DepositOfferId);
            var calc = new TrafficOfAccountCalculator(_keeperDataModel, accountModel,
                new Period(accountModel.Deposit.StartDate, DateTime.Now));
            var isAddOpen = IsAddOpen(accountModel.Deposit, depoOffer, out var addLimitStr);
            var rate = depoOffer.GetCurrentRate(accountModel.Deposit.StartDate, out string formula);
            return new DepositVm()
            {
                Id = accountModel.Id,
                BankName = depoOffer.Bank.Name,
                MainCurrency = depoOffer.MainCurrency,
                DepoName = accountModel.Name,
                RateType = depoOffer.RateType,
                Rate = rate,
                RateFormula = formula,
                AdditionsStr = addLimitStr,
                IsAddOpen = isAddOpen,
                StartDate = accountModel.Deposit.StartDate,
                FinishDate = accountModel.Deposit.FinishDate,
                Balance = calc.EvaluateBalance(),
            };
        }

        private bool IsAddOpen(Deposit deposit, DepositOfferModel depositOffer, out string addLimitString)
        {
            if (deposit.IsAdditionsBanned) // по условия можно, но банк закрыл досрочно
            {
                addLimitString = "банк закрыл досрочно";
                return false;
            }

            if (!depositOffer.IsAddLimited)  // допы не ограничены (карточка, тек счет)
            {
                addLimitString = "без ограничений";
                return true;
            }

            if (depositOffer.AddLimitInDays == 0) // допы не были предусмотренны вообще
            {
                addLimitString = "не предусмотрены";
                return false;
            }

            // допы ограничены по сроку
            var addLimit = deposit.StartDate.AddDays(depositOffer.AddLimitInDays);
            if (addLimit > DateTime.Today)  // срок еще не вышел
            {
                addLimitString = $"открыты до {addLimit:dd/MM/yyyy}";
                return true;
            }

            // срок истек
            addLimitString = $"закрыты с {addLimit:dd/MM/yyyy}";
            return false;
        }

    }

    public static class DepositOfferModelExt
    {
        public static decimal GetCurrentRate(this DepositOfferModel depositOfferModel, DateTime openingDate, out string rateFormula)
        {
            var key = depositOfferModel.CondsMap.Keys.First();
            foreach (var condsMapKey in depositOfferModel.CondsMap.Keys.TakeWhile(condsMapKey => condsMapKey <= openingDate))
            {
                key = condsMapKey;
            }
            var conditions = depositOfferModel.CondsMap[key];

            rateFormula = depositOfferModel.RateType != RateType.Linked ? "" : conditions.RateFormula;
            return conditions.RateLines.Last().Rate;
        }
    }
}
