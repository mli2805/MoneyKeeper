using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keeper2018
{
    public static class TransactionsOldTxt
    {
        public static async Task<List<Transaction>> LoadFromOldTxtAsync(List<Account> accountsPlaneList)
        {
            await Task.Delay(1);
            return LoadFromOldTxt(accountsPlaneList).ToList();
        }

        private static IEnumerable<Transaction> LoadFromOldTxt(List<Account> accountsPlaneList)
        {
            var content = File.ReadAllLines(DbUtils.GetTxtFullPath("TransWithTags.txt"),
                Encoding.GetEncoding("Windows-1251"));

            foreach (var line in content)
            {
                var tran = Parse(line, accountsPlaneList);
                yield return tran;
            }
        }

        private static Transaction Parse(string line, List<Account> accountsPlaneList)
        {
            var tran = new Transaction();
            var substrings = line.Split(';');
            tran.Timestamp = Convert.ToDateTime(substrings[0], new CultureInfo("ru-RU"));
            tran.Operation = (OperationType)Enum.Parse(typeof(OperationType), substrings[1]);
            tran.MyAccount = accountsPlaneList.First(account => account.Header == substrings[2].Trim());
            tran.MySecondAccount = substrings[3].Trim() != "" ? accountsPlaneList.First(account => account.Header == substrings[3].Trim()) : null;
            tran.Amount = Convert.ToDouble(substrings[4], new CultureInfo("en-US"));
            tran.Currency = (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[5]);
            tran.AmountInReturn = Convert.ToDouble(substrings[6], new CultureInfo("en-US"));
            tran.CurrencyInReturn = substrings[7].Trim() != "" ? (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[7]) : CurrencyCode.USD;
            tran.Tags = TagsFromString(substrings[8].Trim(), accountsPlaneList);
            tran.Comment = substrings[9].Trim();

            return tran;
        }

        private static List<Account> TagsFromString(string str, List<Account> accountsPlaneList)
        {
            if (str == "") return new List<Account>();

            var substrings = str.Split('|');
            return substrings.Select(substring => accountsPlaneList.First(account => account.Header == substring.Trim())).ToList();
        }
    }
}