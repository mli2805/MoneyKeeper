using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;

namespace Keeper.DomainModel.Trans
{
    public static class TranExtentions
    {
        public static TranWithTags Clone(this TranWithTags tran)
        {
            var result = new TranWithTags
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
                result.Tags = new List<Account>();
                foreach (var tag in tran.Tags)
                {
                    result.Tags.Add(tag);
                }
            }
            result.Comment = tran.Comment;
            return result;
        }

        public static void CopyInto(this TranWithTags tran, TranWithTags destinationTran)
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
                if (destinationTran.Tags == null) destinationTran.Tags = new List<Account>();
                foreach (var tag in tran.Tags)
                {
                    destinationTran.Tags.Add(tag);
                }
            }
            destinationTran.Comment = tran.Comment;
        }

        public static Brush TranFontColor(this TranWithTags tran)
        {
            if (tran.Operation == OperationType.Доход) return Brushes.Blue;
            if (tran.Operation == OperationType.Расход) return Brushes.Red;
            if (tran.Operation == OperationType.Перенос) return Brushes.Black;
            if (tran.Operation == OperationType.Обмен) return Brushes.Green;
            return Brushes.Gray;
        }

        private static Account UpToCategory(Account tag, string root)
        {
            return tag.Parent.Name == root ? tag : UpToCategory(tag.Parent, root);
        }

        // возвращает базовую категорию
        public static Account GetTranCategory(this TranWithTags tran, bool flag)
        {
            var rootName = flag ? "Все доходы" : "Все расходы";
            var result = tran.Tags.FirstOrDefault(t => t.Is(rootName));
            if (result != null) return UpToCategory(result, rootName);
            MessageBox.Show($"не задана категория {tran.Timestamp} {tran.Amount} {tran.Currency}", "");
            return null;
        }


        // возвращает подробную категорию
        public static Account GetTranArticle(this TranWithTags tran, bool isIncome, bool batchProcessing = true)
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

        public static bool HasntGotCategoryTagThoughItShould(this TranWithTags tran)
        {
            return ((tran.Operation == OperationType.Доход || tran.Operation == OperationType.Расход) &&
                    tran.GetTranArticle(tran.Operation == OperationType.Доход, false) == null);

        }

    }
}
