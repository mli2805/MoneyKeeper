using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KeeperDomain;

namespace Keeper2018
{
    public static class TransactionsOldTxt
    {
        public static async Task<Dictionary<int, Transaction>> LoadFromOldTxtAsync()
        {
            await Task.Delay(1);
            var result = new Dictionary<int, Transaction>();
            var ordinal = 1;
            foreach (var transaction in LoadFromOldTxt())
            {
                result.Add(ordinal, transaction);
                ordinal++;
            }
            return result;
        }

        private static IEnumerable<Transaction> LoadFromOldTxt()
        {
            var content = File.ReadAllLines(DbIoUtils.GetOldTxtFullPath("TransWithTags.txt"),
                Encoding.GetEncoding("Windows-1251"));

            var minutes = 1;
            var date = new DateTime();
            foreach (var line in content)
            {
                var tran = Parse(line);

                if (date.Date != tran.Timestamp.Date)
                    minutes = 1;
                else minutes++;
                tran.Timestamp = tran.Timestamp.Date.AddMinutes(minutes);
                date = tran.Timestamp.Date;

                yield return tran;
            }
        }

        private static Transaction Parse(string line)
        {
            var tran = new Transaction();
            var substrings = line.Split(';');
            tran.Timestamp = Convert.ToDateTime(substrings[0], new CultureInfo("ru-RU"));
            tran.Operation = (OperationType)Enum.Parse(typeof(OperationType), substrings[1]);

            tran.Receipt = int.Parse(substrings[2].Trim());

            tran.MyAccount = int.Parse(substrings[3].Trim());
            tran.MySecondAccount = int.Parse(substrings[4].Trim());

            tran.Amount = Convert.ToDecimal(substrings[5], new CultureInfo("en-US"));
            tran.Currency = (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[6]);
            tran.AmountInReturn = Convert.ToDecimal(substrings[7], new CultureInfo("en-US"));
            tran.CurrencyInReturn = substrings[8].Trim() != "" ? (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[8]) : CurrencyCode.USD;

            tran.Tags = TagsFromString(substrings[9].Trim());

            tran.Comment = substrings[10].Trim();

            return tran;
        }

        private static List<int> TagsFromString(string str)
        {
            if (str == "") return new List<int>();

            var substrings = str.Split('|');
            return substrings.Select(substring => int.Parse(substring.Trim())).ToList();
        }
    }
}