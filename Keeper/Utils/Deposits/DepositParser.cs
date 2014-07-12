using System;
using System.Collections.ObjectModel;
using System.Composition;
using System.Globalization;
using System.Windows;
using Keeper.DomainModel;
using Keeper.Utils.Accounts;

namespace Keeper.Utils.Deposits
{
    [Export]
    public class DepositParser
    {
        private readonly KeeperDb _db;
        private readonly AccountTreeStraightener _accountTreeStraightener;

        [ImportingConstructor]
        public DepositParser(KeeperDb db, AccountTreeStraightener accountTreeStraightener)
        {
            _db = db;
            _accountTreeStraightener = accountTreeStraightener;
        }

        /// <summary>
        /// из предположения, что обратные слэши только в датах, и даты с обеих сторон имеют пробелы
        /// </summary>
        public void ExtractInfoFromName(Account account)
        {
            var s = account.Name;
            var n = s.IndexOf(' ');
            var bankName = s.Substring(0, n);
            var banks = _accountTreeStraightener.Seek("Банки", _db.Accounts);
            foreach (var bank in banks.Children)
            {
                if (bank.Name.Substring(0, 3) != bankName.Substring(0, 3)) continue;
                account.Deposit.Bank = bank;
                break;
            }
            if (account.Deposit.Bank == null) MessageBox.Show(bankName);

            s = s.Substring(n + 1);
            var p = s.IndexOf('/');
            account.Deposit.Title = s.Substring(0, p - 2);

            n = s.IndexOf(' ', p);
            account.Deposit.StartDate = Convert.ToDateTime(s.Substring(p - 2, n - p + 2), new CultureInfo("ru-RU"));
            p = s.IndexOf('/', n);
            n = s.IndexOf(' ', p);
            account.Deposit.FinishDate = Convert.ToDateTime(s.Substring(p - 2, n - p + 2), new CultureInfo("ru-RU"));
            p = s.IndexOf('%', n);
            account.Deposit.DepositRateLines = new ObservableCollection<DepositRateLine> { new DepositRateLine { AmountFrom = 0, AmountTo = 999999999999, DateFrom = account.Deposit.StartDate, Rate = Convert.ToDecimal(s.Substring(n, p - n)) } };
        }


    }
}