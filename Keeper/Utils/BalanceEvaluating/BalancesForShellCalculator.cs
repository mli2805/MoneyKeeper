using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.Rates;

namespace Keeper.Utils.BalanceEvaluating
{
    [Export]
    public class BalancesForShellCalculator
    {
        private readonly AccountBalanceCalculator _accountBalanceCalculator;
        private readonly ArticleBalanceCalculator _articleBalanceCalculator;
        private readonly RateExtractor _rateExtractor;

        [ImportingConstructor]
        public BalancesForShellCalculator(RateExtractor rateExtractor, AccountBalanceCalculator accountBalanceCalculator, ArticleBalanceCalculator articleBalanceCalculator)
        {
            _rateExtractor = rateExtractor;
            _accountBalanceCalculator = accountBalanceCalculator;
            _articleBalanceCalculator = articleBalanceCalculator;
        }

        private List<string> OneBalance(Account balancedAccount, Period period, out decimal totalInUsd)
        {
            var balance = new List<string>();
            totalInUsd = 0;
            if (balancedAccount == null) return balance;

            var balancePairs = _accountBalanceCalculator.GetAccountBalancePairs(balancedAccount, period.ExpandFromMidnightToMidnight());

            foreach (var item in balancePairs)
            {
                if (item.Amount != 0)
                    balance.Add(item.Currency == CurrencyCodes.BYR
                        ? String.Format("{0:#,#} {1}", item.Amount, item.Currency)
                        : String.Format("{0:#,0.##} {1}", item.Amount, item.Currency));
                totalInUsd += _rateExtractor.GetUsdEquivalent(item.Amount, item.Currency, period.Finish);
            }

            return balance;
        }

        private decimal FillListWithBalance(Account selectedAccount, Period period, ObservableCollection<string> balanceList)
        {
            decimal inUsd;
            var b = OneBalance(selectedAccount, period, out inUsd);
            foreach (var st in b)
                balanceList.Add(st);

            foreach (var child in selectedAccount.Children)
            {
                decimal temp;
                b = OneBalance(child, period, out temp);
                if (b.Count > 0) balanceList.Add("         " + child.Name);
                foreach (var st in b)
                    balanceList.Add("    " + st);
            }
            return inUsd;
        }

        private decimal FillListWithTraffic(Account selectedAccount, Period period, ObservableCollection<string> trafficList)
        {
            trafficList.Clear();
            var firstTransactions = new List<string>();

            var b = selectedAccount.Is("Внешние") ?
                _accountBalanceCalculator.GetAccountSaldoInUsdPlusTransactions(selectedAccount, period.ExpandFromMidnightToMidnight(), firstTransactions) :
                _articleBalanceCalculator.GetArticleSaldoInUsdPlusTransactions(selectedAccount, period.ExpandFromMidnightToMidnight(), firstTransactions);
            trafficList.Add(b == 0
                              ? "В данном периоде \nдвижение по выбранному счету не найдено"
                              : string.Format("{0}   {1:#,0} usd", selectedAccount.Name, b));

            foreach (var child in selectedAccount.Children)
            {
                var c = selectedAccount.Is("Внешние") ?
                    _accountBalanceCalculator.GetAccountSaldoInUsdPlusTransactions(child, period.ExpandFromMidnightToMidnight(), firstTransactions) :
                    _articleBalanceCalculator.GetArticleSaldoInUsdPlusTransactions(child, period.ExpandFromMidnightToMidnight(), firstTransactions);
                if (c != 0) trafficList.Add(string.Format("   {0}   {1:#,0} usd", child.Name, c));
            }

            if (selectedAccount.Children.Count == 0)
                foreach (var transaction in firstTransactions)
                    trafficList.Add(transaction);

            return b;
        }

        public decimal FillListForShellView(Account selectedAccount, Period period, ObservableCollection<string> balanceList)
        {
            balanceList.Clear();
            if (selectedAccount == null) return 0;
            return (selectedAccount.Is("Мои")) ?
                FillListWithBalance(selectedAccount, period, balanceList) :
                FillListWithTraffic(selectedAccount, period, balanceList);
        }
    }
}
