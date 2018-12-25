using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Keeper2018
{
    public static class TranExtentions
    {
        public static TransactionModel Clone(this TransactionModel tran)
        {
            var result = new TransactionModel
            {
                Timestamp = tran.Timestamp,
                Operation = tran.Operation,
                MyAccount = tran.MyAccount,
                MySecondAccount = tran.MySecondAccount,
                Amount = tran.Amount,
                Currency = tran.Currency,
                AmountInReturn = tran.AmountInReturn,
                CurrencyInReturn = tran.CurrencyInReturn
            };
            if (tran.Tags != null)
            {
                result.Tags = new List<AccountModel>();
                foreach (var tag in tran.Tags)
                {
                    result.Tags.Add(tag);
                }
            }
            result.Comment = tran.Comment;
            return result;
        }

        public static void CopyInto(this TransactionModel tran, TransactionModel destinationTran)
        {
            destinationTran.Timestamp = tran.Timestamp;
            destinationTran.Operation = tran.Operation;
            destinationTran.MyAccount = tran.MyAccount;
            destinationTran.MySecondAccount = tran.MySecondAccount;
            destinationTran.Amount = tran.Amount;
            destinationTran.Currency = tran.Currency;
            destinationTran.AmountInReturn = tran.AmountInReturn;
            destinationTran.CurrencyInReturn = tran.CurrencyInReturn;

            destinationTran.Tags?.Clear();
            if (tran.Tags != null)
            {
                if (destinationTran.Tags == null) destinationTran.Tags = new List<AccountModel>();
                foreach (var tag in tran.Tags)
                {
                    destinationTran.Tags.Add(tag);
                }
            }
            destinationTran.Comment = tran.Comment;
        }
        private static AccountModel UpToCategory(AccountModel tag, string root)
        {
            return tag.Owner.Name == root ? tag : UpToCategory(tag.Owner, root);
        }
        // возвращает базовую категорию
        public static AccountModel GetTranCategory(this TransactionModel tran, bool flag)
        {
            var rootName = flag ? "Все доходы" : "Все расходы";
            var result = tran.Tags.FirstOrDefault(t => t.Is(rootName));
            if (result != null) return UpToCategory(result, rootName);
            MessageBox.Show($"не задана категория {tran.Timestamp} {tran.Amount} {tran.Currency}", "");
            return null;
        }
        // возвращает подробную категорию
        public static AccountModel GetTranArticle(this TransactionModel tran, bool isIncome, bool batchProcessing = true)
        {
            var rootName = isIncome ? "Все доходы" : "Все расходы";
            var result = tran.Tags.FirstOrDefault(t => t.Is(rootName));
            if (result != null) return result;
            MessageBox.Show(
                batchProcessing
                    ? $"Нет категории для проводки \n {tran.Timestamp} {tran.Amount} {tran.Currency.ToString().ToLower()}"
                    : "Не задана категория!", "Ошибка!");
            return null;

        }
        public static bool HasntGotCategoryTagThoughItShould(this TransactionModel tran)
        {
            return ((tran.Operation == OperationType.Доход || tran.Operation == OperationType.Расход) &&
                    tran.GetTranArticle(tran.Operation == OperationType.Доход, false) == null);
        }

        public static CurrencyCode CurrencyOfSecondAccount(this TransactionModel tran)
        {
            return tran.Operation == OperationType.Перенос
                ? tran.Currency
                : tran.CurrencyInReturn.GetValueOrDefault();
        }

        public static string DumpOfSecondAccount(this TransactionModel tran)
        {
            return tran.Operation == OperationType.Перенос || tran.Operation == OperationType.Обмен
                ? tran.MySecondAccount.Name
                : "";
        }
    }
}
