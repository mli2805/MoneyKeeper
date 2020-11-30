using System.Collections.Generic;
using System.Linq;
using System.Windows;
using KeeperDomain;

namespace Keeper2018
{
    public static class TranExtentions
    {
        public static TransactionModel Clone(this TransactionModel tran)
        {
            var result = new TransactionModel
            {
                TransactionKey = tran.TransactionKey,
                Timestamp = tran.Timestamp,
                Operation = tran.Operation,
                PaymentWay = tran.PaymentWay,
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
            destinationTran.TransactionKey = tran.TransactionKey;
            destinationTran.Timestamp = tran.Timestamp;
            destinationTran.Operation = tran.Operation;
            destinationTran.PaymentWay = tran.PaymentWay;
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

        // возвращает подробную категорию
        private static AccountModel GetTranArticle(this TransactionModel tran, bool isIncome, bool batchProcessing = true)
        {
            var rootId = isIncome ? 185 : 189;
            var category = tran.Tags.FirstOrDefault(t => t.Is(rootId));
            if (category != null) return category;
            MessageBox.Show(
                batchProcessing
                    ? $"Нет категории для проводки \n {tran.Timestamp} {tran.Amount} {tran.Currency.ToString().ToLower()}"
                    : "Не задана категория!", "Ошибка!");
            return null;

        }

        public static AccountModel GetExternalAccount(this TransactionModel tran)
        {
            var externalAccount = tran.Tags.FirstOrDefault(a => a.Is(157));
            if (externalAccount == null)
                MessageBox.Show(tran.Operation == OperationType.Расход 
                    ? "Должен быть хотя бы один продавец/услугодатель" 
                    : "Должен быть хотя бы один плательщик");

            return externalAccount;
        }

        public static bool HasntGotCategoryTagThoughItShould(this TransactionModel tran)
        {
            if (tran.Operation != OperationType.Доход && tran.Operation != OperationType.Расход) return false; // OK
            return tran.GetExternalAccount() == null ||
                   tran.GetTranArticle(tran.Operation == OperationType.Доход, false) == null;
        }

    }
}
