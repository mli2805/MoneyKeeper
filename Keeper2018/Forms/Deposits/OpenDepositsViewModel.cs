using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class OpenDepositsViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        public List<DepositVm> Rows { get; private set; }

        public List<DepoTotalVm> Totals { get; private set; }

        public OpenDepositsViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Действующие депозиты";
        }

        public void Initialize()
        {
            Rows = _dataModel.GetOpenDepositsOrderedByFinishDate().Select(Convert).ToList();

            EvaluateDepoTotals();
            EvaluateDepoAndMatras();
            EvaluateAllMine();
            foreach (var l in Totals.Where(t => t.AllMine.SumUsd == 0).ToList())
                Totals.Remove(l);
        }

        private void EvaluateDepoAndMatras()
        {
            var matras = _dataModel.AcMoDict[167]; // шкаф
            var calc = new TrafficOfBranchCalculator(_dataModel, matras,
                new Period() { FinishMoment = DateTime.Now });
            var balance = calc.Evaluate();

            decimal total = 0;
            foreach (var pair in balance.Currencies)
            {
                var line = Totals.First(l => l.Currency == pair.Key && !l.IsAggregateLine);
                line.DepoAndMatras.Currency = pair.Key;
                var sum = line.Depo.Sum + pair.Value;
                line.DepoAndMatras.Sum = sum;
                var inUsd = line.Currency == CurrencyCode.USD ? sum : _dataModel.AmountInUsd(DateTime.Now, pair.Key, sum);
                total += inUsd;
                line.DepoAndMatras.SumUsd = inUsd;
            }

            var notInMatras = Totals.Where(t => t.DepoAndMatras.SumUsd == 0 && !t.IsAggregateLine).ToList();
            foreach (var line in notInMatras)
            {
                line.DepoAndMatras = line.Depo.Clone();
                total += line.Depo.SumUsd;
            }

            var full = Totals.First(l => l.IsAggregateLine);
            full.DepoAndMatras.SumUsd = total;
            full.DepoAndMatras.Currency = CurrencyCode.USD;

            foreach (var line in Totals.Where(t=>t.DepoAndMatras.Sum > 0))
                line.DepoAndMatras.Percent = line.DepoAndMatras.SumUsd / total;
        }

        private void EvaluateAllMine()
        {
            var allMine = _dataModel.AcMoDict[158]; // мои
            var calc = new TrafficOfBranchCalculator(_dataModel, allMine,
                new Period() { FinishMoment = DateTime.Now });
            var balance = calc.Evaluate();

            decimal total = 0;
            foreach (var pair in balance.Currencies)
            {
                var line = Totals.First(l => !l.IsAggregateLine && l.Currency == pair.Key);
                line.AllMine.Currency = pair.Key;
                line.AllMine.Sum = pair.Value;
                var inUsd = _dataModel.AmountInUsd(DateTime.Now, pair.Key, pair.Value);
                total += inUsd;
                line.AllMine.SumUsd = inUsd;
            }

            var full = Totals.First(l => l.IsAggregateLine);
            full.AllMine.SumUsd = total;
            full.AllMine.Currency = CurrencyCode.USD;

            foreach (var line in Totals.Where(t=>t.AllMine.Sum > 0))
                line.AllMine.Percent = line.AllMine.SumUsd / total;
        }

        private void EvaluateDepoTotals()
        {
            Totals = new List<DepoTotalVm>();

            int totalCount = 0;
            decimal total = 0;
            foreach (var currency in Enum.GetValues(typeof(CurrencyCode)).OfType<CurrencyCode>())
            {
                var line = new DepoTotalVm() { Currency = currency };

                var dcs = Rows.Where(r => r.MainCurrency == currency).ToList();
                var sum = dcs.Sum(d => d.Balance.Currencies[currency]);
                if (sum > 0)
                {
                    totalCount += dcs.Count;
                    var amountInUsd = _dataModel.AmountInUsd(DateTime.Now, currency, sum);
                    total += amountInUsd;

                    line.Depo.Pieces = dcs.Count;
                    line.Depo.Sum = sum;
                    line.Depo.SumUsd = amountInUsd;
                    line.Depo.Currency = currency;
                }

                Totals.Add(line);
            }
            var full = new DepoTotalVm() { Currency = CurrencyCode.USD, IsAggregateLine = true };
            full.Depo.Pieces = totalCount;
            full.Depo.SumUsd = total;
            full.Depo.Currency = CurrencyCode.USD;

            foreach (var line in Totals.Where(l => l.Depo.Sum > 0))
                line.Depo.Percent = line.Depo.SumUsd / full.Depo.SumUsd;

            Totals.Add(full);
        }


        private DepositVm Convert(AccountItemModel accountItemModel)
        {
            var depoOffer = _dataModel.DepositOffers
                .First(o => o.Id == accountItemModel.BankAccount.DepositOfferId);
            var calc = new TrafficOfAccountCalculator(_dataModel, accountItemModel,
                new Period(accountItemModel.BankAccount.StartDate, DateTime.Now));
            var isAddOpen = IsAddOpen(accountItemModel.BankAccount, depoOffer, out var addLimitStr);
            var rate = depoOffer.GetCurrentRate(accountItemModel.BankAccount.StartDate, out string formula);
            return new DepositVm()
            {
                Id = accountItemModel.Id,
                BankName = depoOffer.Bank.Name,
                MainCurrency = depoOffer.MainCurrency,
                DepoName = accountItemModel.Name,
                RateType = depoOffer.RateType,
                Rate = rate,
                RateFormula = formula,
                AdditionsStr = addLimitStr,
                IsAddOpen = isAddOpen,
                StartDate = accountItemModel.BankAccount.StartDate,
                FinishDate = accountItemModel.BankAccount.FinishDate,
                Balance = calc.EvaluateBalance(),
            };
        }

        private bool IsAddOpen(BankAccountModel bankAccountModel, DepositOfferModel depositOffer, out string addLimitString)
        {
            if (bankAccountModel.Deposit.IsAdditionsBanned) // по условия можно, но банк закрыл досрочно
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
            var addLimit = bankAccountModel.StartDate.AddDays(depositOffer.AddLimitInDays);
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
}
