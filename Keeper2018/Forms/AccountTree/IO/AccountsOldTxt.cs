using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Keeper2018
{
    public static class AccountsOldTxt
    {
        public static IEnumerable<Account> LoadFromOldTxt()
        {
            var deposits = LoadDepositsFromOldTxt().ToList();
            return File.ReadAllLines(DbUtils.GetOldTxtFullPath("Accounts.txt"), Encoding.GetEncoding("Windows-1251")).
                Where(s => !string.IsNullOrWhiteSpace(s)).Select(l => AccountFromString(l, deposits));
        }

        private static Account AccountFromString(string s, List<Deposit> deposits)
        {
            var substrings = s.Split(';');
            var account = new Account
            {
                Id = Convert.ToInt32(substrings[0]),
                Header = substrings[1].Trim(),
                OwnerId = Convert.ToInt32(substrings[2]),
                IsFolder = Convert.ToBoolean(substrings[3]),
                //IsClosed = Convert.ToBoolean(substrings[4]),
                IsExpanded = Convert.ToBoolean(substrings[5]),
            };
            account.Deposit = deposits.FirstOrDefault(d => d.MyAccountId == account.Id);
            return account;
        }

        private static IEnumerable<Deposit> LoadDepositsFromOldTxt()
        {
            return File.ReadAllLines(DbUtils.GetOldTxtFullPath("Deposits.txt"), Encoding.GetEncoding("Windows-1251"))
                .Where(s => !string.IsNullOrWhiteSpace(s)).Select(DepositFromString);
        }

        private static Deposit DepositFromString(string s)
        {
            var substrings = s.Split(';');
            Deposit deposit = new Deposit()
            {
                MyAccountId = Convert.ToInt32(substrings[0]),
                DepositOfferId = Convert.ToInt32(substrings[1]),
                Serial = substrings[2].Trim(),
                StartDate = Convert.ToDateTime(substrings[3], new CultureInfo("ru-RU")),
                FinishDate = Convert.ToDateTime(substrings[4], new CultureInfo("ru-RU")),
                ShortName = substrings[5].Trim(),
                Comment = substrings[6].Replace("|", "\r\n"),
            };
            return deposit;
        }
    }
}