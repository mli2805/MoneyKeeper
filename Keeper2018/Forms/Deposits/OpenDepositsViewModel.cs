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
                .OrderBy(d=>d.Deposit.FinishDate)
                .Select(Convert).ToList();

            Footer = new List<string>();
            int totalCount = 0;
            decimal total = 0;
            foreach (var currency in Enum.GetValues(typeof(CurrencyCode)).OfType<CurrencyCode>())
            {
                var dcs = Rows.Where(r => r.MainCurrency == currency).ToList();
                var sum = dcs.Sum(d=>d.Balance.Currencies[currency]);
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
                new Period(accountModel.Deposit.StartDate, DateTime.Today));
            return new DepositVm()
            {
                Id = accountModel.Id,
                BankName = depoOffer.Bank.Name,
                MainCurrency = depoOffer.MainCurrency,
                DepoName = accountModel.Name,
                RateTypeStr = depoOffer.RateType.ToString(),
                AdditionsStr = depoOffer.AddLimitStr, // TODO брать из конкретного вклада, с учетом даты открытия и запретов банка
                StartDate = accountModel.Deposit.StartDate,
                FinishDate = accountModel.Deposit.FinishDate,
                Balance = calc.EvaluateBalance(),
            };
        }
    }
}
